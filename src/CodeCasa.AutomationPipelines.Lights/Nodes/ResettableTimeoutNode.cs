using CodeCasa.AutomationPipelines.Lights.Utils;
using CodeCasa.Lights;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class ResettableTimeoutNode : LightTransitionNode
{
    private readonly IPipelineNode<LightTransition> _childNode;
    private readonly CompositeDisposable _disposables = new();
    private readonly SerialDisposable _timerSubscription = new();
    private bool _isPersisting;
    private bool _hasHandedOver;
    private int _timerGeneration;
    private int _isChildDisposed;
    private bool _isDisposed;

    public ResettableTimeoutNode(IPipelineNode<LightTransition> childNode, TimeSpan timeout,
        IObservable<bool> persistObservable, IScheduler scheduler,
        TimeoutBehaviour timeoutBehaviour = TimeoutBehaviour.TurnOff) : base(scheduler)
    {
        _childNode = childNode;
        var childName = childNode.Name ?? childNode.ToString();
        Name = timeoutBehaviour == TimeoutBehaviour.PassThrough
            ? $"{childName} (passes through after timeout)"
            : $"{childName} (resets after timeout)";
        _timerSubscription.DisposeWith(_disposables);

        // The initial output is set synchronously: a reactive node reads Output right after activating this node, and would
        // otherwise briefly pass null downstream until the scheduler delivered it.
        Output = childNode.Output;
        RestartTimer();

        /*
         * The scheduler may run the callbacks below and the timer on different threads. Each one runs serialised with the
         * node's input, and checks _hasHandedOver and the timer generation there: disposing a subscription does not stop
         * a callback that already started, and such a callback must not claim the light again after a hand-over.
         */
        childNode.OnNewOutput
            .ObserveOn(scheduler)
            .Subscribe(output => RunSerialized(() =>
            {
                if (_hasHandedOver)
                {
                    return;
                }

                Output = output;
                RestartTimer();
            })).DisposeWith(_disposables);

        persistObservable
            .ObserveOn(scheduler)
            .DistinctUntilChanged()
            .Subscribe(persist => RunSerialized(() =>
            {
                if (_hasHandedOver)
                {
                    return;
                }

                _isPersisting = persist;
                if (persist)
                {
                    _timerGeneration++;
                    _timerSubscription.Disposable = null;
                }
                else
                {
                    RestartTimer();
                }
            })).DisposeWith(_disposables);

        void RestartTimer()
        {
            if (_isPersisting)
            {
                return;
            }

            var generation = ++_timerGeneration;
            _timerSubscription.Disposable = Observable.Timer(timeout, scheduler)
                .Subscribe(_ => RunSerialized(() =>
                {
                    if (generation == _timerGeneration)
                    {
                        OnTimeout();
                    }
                }));
        }

        void OnTimeout()
        {
            if (timeoutBehaviour == TimeoutBehaviour.TurnOff)
            {
                ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition.Off());
                return;
            }

            // Passing through ends the override for good: a later child output or persist change must not claim the light again.
            _hasHandedOver = true;
            _disposables.Dispose();
            PassInputThrough();
            DisposeChildNode().GetAwaiter().GetResult();
        }
    }

    protected override void OnInputChanged(LightTransition? input)
    {
        if (_hasHandedOver)
        {
            return;
        }

        _childNode.Input = input;
    }

    private Task DisposeChildNode()
    {
        // A hand-over on the scheduler thread and DisposeAsync can get here at the same time.
        return Interlocked.Exchange(ref _isChildDisposed, 1) == 0
            ? _childNode.DisposeOrDisposeAsync()
            : Task.CompletedTask;
    }

    public override async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        _disposables.Dispose();
        await DisposeChildNode();
        await base.DisposeAsync();
    }
}

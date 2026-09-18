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
    private bool _isDisposed;

    public ResettableTimeoutNode(IPipelineNode<LightTransition> childNode, TimeSpan turnOffTime,
        IObservable<bool> persistObservable, IScheduler scheduler,
        TimeoutBehaviour timeoutBehaviour = TimeoutBehaviour.TurnOff) : base(scheduler)
    {
        _childNode = childNode;
        Name = timeoutBehaviour == TimeoutBehaviour.PassThrough
            ? $"{childNode} (passes through after timeout)"
            : $"{childNode} (resets after timeout)";
        _timerSubscription.DisposeWith(_disposables);

        // The initial output is set synchronously: a reactive node reads Output right after activating this node, and would
        // otherwise briefly pass null downstream until the scheduler delivered it.
        Output = childNode.Output;
        RestartTimer();

        childNode.OnNewOutput
            .ObserveOn(scheduler)
            .Subscribe(output =>
            {
                Output = output;
                RestartTimer();
            }).DisposeWith(_disposables);

        persistObservable
            .ObserveOn(scheduler)
            .DistinctUntilChanged()
            .Subscribe(persist =>
            {
                _isPersisting = persist;
                if (persist)
                {
                    _timerSubscription.Disposable = null;
                }
                else
                {
                    RestartTimer();
                }
            }).DisposeWith(_disposables);

        void RestartTimer()
        {
            if (_isPersisting)
            {
                return;
            }

            _timerSubscription.Disposable = Observable.Timer(turnOffTime, scheduler)
                .Subscribe(_ => OnTimeout());
        }

        void OnTimeout()
        {
            if (timeoutBehaviour == TimeoutBehaviour.TurnOff)
            {
                ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition.Off());
                return;
            }

            // Passing through ends the override for good: a later child output or persist change must not claim the light again.
            _disposables.Dispose();
            PassInputThrough();
        }
    }

    protected override void OnInputChanged(LightTransition? input)
    {
        _childNode.Input = input;
    }

    public override async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        _disposables.Dispose();
        await _childNode.DisposeAsync();
        await base.DisposeAsync();
    }
}

using CodeCasa.Lights;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    internal class ResettableTimeoutNode : LightTransitionNode, IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private IDisposable? _timerSubscription;
        private bool _isPersisting;

        public ResettableTimeoutNode(IPipelineNode<LightTransition> childNode, TimeSpan turnOffTime,
            IObservable<bool> persistObservable, IScheduler scheduler) : base(scheduler)
        {
            Name = $"{childNode.Name} (resets after timeout)";

            childNode.OnNewOutput
                .Prepend(childNode.Output)
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
                    _timerSubscription?.Dispose();
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

                _timerSubscription?.Dispose();
                _timerSubscription = Observable.Timer(turnOffTime, scheduler)
                    .Subscribe(_ => ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition.Off()));
            }
        }

        public void Dispose() => _disposables.Dispose();
    }
}

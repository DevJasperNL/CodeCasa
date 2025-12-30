using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    internal class ResettableTimeoutNode<T> : LightTransitionNode{
        public ResettableTimeoutNode(IPipelineNode<LightTransition> childNode, TimeSpan turnOffTime,
            IObservable<T> refreshObservable, IScheduler scheduler) : base(scheduler)
        {
            var serializedChild = childNode.OnNewOutput.Prepend(childNode.Output).ObserveOn(scheduler);

            var serializedTurnOff =
                refreshObservable.Select(_ => Unit.Default)
                    .Prepend(Unit.Default)
                    .Throttle(turnOffTime, scheduler)
                    .Take(1)
                    .ObserveOn(scheduler);

            serializedChild
                .TakeUntil(serializedTurnOff)
                .Subscribe(output => { Output = output; });

            serializedTurnOff
                .Subscribe(_ => { ChangeOutputAndTurnOnPassThroughOnNextInput(LightTransition.Off()); });
        }
    }
}

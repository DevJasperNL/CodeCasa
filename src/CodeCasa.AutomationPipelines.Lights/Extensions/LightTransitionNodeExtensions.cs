using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    public static class LightTransitionNodeExtensions
    {
        public static IPipelineNode<LightTransition> TurnOffAfter(this IPipelineNode<LightTransition> node,
            TimeSpan timeSpan, IScheduler scheduler)
        {
            return new ResettableTimeoutNode<Unit>(node, timeSpan, Observable.Empty<Unit>(), scheduler);
        }

        public static IPipelineNode<LightTransition> TurnOffAfter<T>(this IPipelineNode<LightTransition> node,
            TimeSpan timeSpan, IObservable<T> resetTimerObservable, IScheduler scheduler)
        {
            return new ResettableTimeoutNode<T>(node, timeSpan, resetTimerObservable, scheduler);
        }
    }
}

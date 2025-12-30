using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IPipelineNode{LightTransition}"/> instances.
    /// These extensions enable adding timeout behavior to light transition nodes.
    /// </summary>
    public static class LightTransitionNodeExtensions
    {
        /// <summary>
        /// Creates a timeout node that automatically turns off the light after the specified time span.
        /// The timeout is not reset by any external events.
        /// </summary>
        /// <param name="node">The pipeline node to wrap with timeout behavior.</param>
        /// <param name="timeSpan">The duration after which the light will turn off.</param>
        /// <param name="scheduler">The scheduler to use for timing operations.</param>
        /// <returns>A new pipeline node that wraps the original node with timeout behavior.</returns>
        public static IPipelineNode<LightTransition> TurnOffAfter(this IPipelineNode<LightTransition> node,
            TimeSpan timeSpan, IScheduler scheduler)
        {
            return new ResettableTimeoutNode<Unit>(node, timeSpan, Observable.Empty<Unit>(), scheduler);
        }

        /// <summary>
        /// Creates a timeout node that automatically turns off the light after the specified time span.
        /// The timeout can be reset when the observable emits a value.
        /// </summary>
        /// <typeparam name="T">The type of values emitted by the reset timer observable.</typeparam>
        /// <param name="node">The pipeline node to wrap with timeout behavior.</param>
        /// <param name="timeSpan">The duration after which the light will turn off.</param>
        /// <param name="resetTimerObservable">An observable that resets the timeout timer when it emits a value.</param>
        /// <param name="scheduler">The scheduler to use for timing operations.</param>
        /// <returns>A new pipeline node that wraps the original node with resettable timeout behavior.</returns>
        public static IPipelineNode<LightTransition> TurnOffAfter<T>(this IPipelineNode<LightTransition> node,
            TimeSpan timeSpan, IObservable<T> resetTimerObservable, IScheduler scheduler)
        {
            return new ResettableTimeoutNode<T>(node, timeSpan, resetTimerObservable, scheduler);
        }
    }
}

using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IPipelineNode{LightTransition}"/> instances.
    /// These extensions enable adding timeout behavior to light transition nodes.
    /// </summary>
    public static class LightTransitionNodeExtensions
    {
        /// <summary>
        /// Wraps the pipeline node to automatically transition to an 'Off' state after a specified duration of inactivity.
        /// </summary>
        /// <param name="node">The source pipeline node.</param>
        /// <param name="timeSpan">The duration to wait before turning off.</param>
        /// <param name="scheduler">The scheduler used for timing operations.</param>
        /// <returns>A new pipeline node that times out to 'Off'.</returns>
        public static IPipelineNode<LightTransition> TurnOffAfter(this IPipelineNode<LightTransition> node,
            TimeSpan timeSpan, IScheduler scheduler)
        {
            return new ResettableTimeoutNode(node, timeSpan, Observable.Empty<bool>(), scheduler);
        }

        /// <summary>
        /// Wraps the pipeline node to automatically transition to an 'Off' state after a specified duration of inactivity, 
        /// with an optional observable to persist the current state and bypass the timeout.
        /// </summary>
        /// <param name="node">The source pipeline node.</param>
        /// <param name="timeSpan">The duration to wait before turning off.</param>
        /// <param name="persistObservable">An observable that, when true, prevents the timeout from triggering.</param>
        /// <param name="scheduler">The scheduler used for timing operations.</param>
        /// <returns>A new pipeline node that times out to 'Off' unless persistence is active.</returns>
        public static IPipelineNode<LightTransition> TurnOffAfter(this IPipelineNode<LightTransition> node,
            TimeSpan timeSpan, IObservable<bool> persistObservable, IScheduler scheduler)
        {
            return new ResettableTimeoutNode(node, timeSpan, persistObservable, scheduler);
        }
    }
}

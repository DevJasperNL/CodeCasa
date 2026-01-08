using CodeCasa.Lights;
using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ILightPipelineContext"/>.
    /// </summary>
    public static class LightPipelineContextExtensions
    {
        /// <summary>
        /// Creates a pipeline node that applies the specified light parameters and automatically turns off the light after a specified duration.
        /// The timeout is not reset by any external events.
        /// </summary>
        /// <param name="context">The light pipeline context.</param>
        /// <param name="lightParameters">The light parameters to apply as a transition.</param>
        /// <param name="timeSpan">The duration after which the light should turn off.</param>
        /// <returns>A pipeline node that applies the light parameters and handles the turn-off behavior.</returns>
        public static IPipelineNode<LightTransition> LightParametersThatTurnsOffAfter(this ILightPipelineContext context,
            LightParameters lightParameters,
            TimeSpan timeSpan)
        {
            var scheduler = context.ServiceProvider.GetRequiredService<IScheduler>();
            var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
            return innerNode.TurnOffAfter(timeSpan, scheduler);
        }

        /// <summary>
        /// Creates a pipeline node that applies the specified light parameters and automatically turns off the light after a specified duration.
        /// The timeout can be reset when the observable emits a value.
        /// </summary>
        /// <typeparam name="T">The type of elements emitted by the reset timer observable.</typeparam>
        /// <param name="context">The light pipeline context.</param>
        /// <param name="lightParameters">The light parameters to apply as a transition.</param>
        /// <param name="timeSpan">The duration after which the light should turn off.</param>
        /// <param name="resetTimerObservable">An observable that resets the turn-off timer when it emits.</param>
        /// <returns>A pipeline node that applies the light parameters and handles the turn-off behavior.</returns>
        public static IPipelineNode<LightTransition> LightParametersThatTurnsOffAfter<T>(this ILightPipelineContext context,
            LightParameters lightParameters,
            TimeSpan timeSpan, IObservable<T> resetTimerObservable)
        {
            var scheduler = context.ServiceProvider.GetRequiredService<IScheduler>();
            var innerNode = new StaticLightTransitionNode(lightParameters.AsTransition(), scheduler);
            return innerNode.TurnOffAfter(timeSpan, resetTimerObservable, scheduler);
        }
    }
}


using System.Reactive.Concurrency;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class SchedulerExtensions
    {
        /// <summary>
        /// Schedules a smooth light transition between two <see cref="LightParameters"/> states,
        /// optionally interpolating progress if the transition is already in progress.
        /// </summary>
        public static IDisposable? ScheduleInterpolatedLightTransition(
            this IScheduler scheduler, 
            LightParameters? sourceLightParameters,
            LightParameters? destinationLightParameters,
            DateTime? startOfTransition,
            DateTime? endOfTransition,
            Action<LightTransition?> transitionAction,
            int defaultTransitionTimeMs = 500)
        {
            if (destinationLightParameters == null)
            {
                transitionAction(null);
                return null;
            }

            if (endOfTransition == null)
            {
                // If there is no end of transition specified, we simply go to the destination state immediately.
                transitionAction(destinationLightParameters.AsTransition());
                return null;
            }

            var utcNow = DateTime.UtcNow;
            // Note: this can be negative.
            var timeToEndOfInputTransition = endOfTransition.Value - utcNow;
            // For any transition under half a second we simply don't provide a transition. Lights will just smoothly go to the corresponding state.
            if (timeToEndOfInputTransition <= TimeSpan.FromMilliseconds(defaultTransitionTimeMs))
            {
                transitionAction(destinationLightParameters.AsTransition());
                return null;
            }

            if (sourceLightParameters == null || startOfTransition == null)
            {
                // We don't know the original parameters or the start of this transition, so we simply transition from where we are towards the new.
                transitionAction(destinationLightParameters.AsTransition(timeToEndOfInputTransition));
                return null;
            }

            var total = (endOfTransition.Value - startOfTransition.Value).TotalSeconds;
            var elapsed = (utcNow - startOfTransition.Value).TotalSeconds;
            var progress = elapsed / total;

            var initialState = sourceLightParameters.Interpolate(destinationLightParameters, progress);
            // First (re)set the lights to where they would have been.
            transitionAction(initialState.AsTransition());
            // After the lights have been reset (default transition takes half a second), continue the rest of the transition.
            return scheduler.Schedule(TimeSpan.FromMilliseconds(defaultTransitionTimeMs),
                () => transitionAction(destinationLightParameters.AsTransition(timeToEndOfInputTransition - TimeSpan.FromMilliseconds(defaultTransitionTimeMs))));
        }
    }
}

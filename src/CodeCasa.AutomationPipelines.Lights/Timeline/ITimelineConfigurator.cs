using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Timeline
{
    /// <summary>
    /// Configurator for building a time-based timeline mapping timeline points to light parameters.
    /// </summary>
    public interface ITimelineConfigurator
    {
        /// <summary>
        /// Adds a mapping from a timeline point to light parameters.
        /// </summary>
        /// <param name="timeline">The timeline point.</param>
        /// <param name="lightParameters">The light parameters associated with the timeline point.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ITimelineConfigurator Add(ITimeline timeline, LightParameters lightParameters);

        /// <summary>
        /// Sets the duration of the initial fade from the current state when entering a timeline state. Defaults to 500ms if not set.
        /// </summary>
        /// <param name="transitionTime">The transition duration.</param>
        /// <returns>The configurator instance for method chaining.</returns>
        ITimelineConfigurator SetTransitionTime(TimeSpan transitionTime);
    }
}

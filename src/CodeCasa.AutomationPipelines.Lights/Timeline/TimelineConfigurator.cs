using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Timeline
{
    internal class TimelineConfigurator : ITimelineConfigurator
    {
        internal Dictionary<ITimeline, LightParameters> Timeline { get; } = [];
        internal TimeSpan? TransitionTime { get; private set; }

        public ITimelineConfigurator Add(ITimeline timeline, LightParameters lightParameters)
        {
            Timeline[timeline] = lightParameters;
            return this;
        }

        public ITimelineConfigurator SetTransitionTime(TimeSpan transitionTime)
        {
            TransitionTime = transitionTime;
            return this;
        }
    }
}

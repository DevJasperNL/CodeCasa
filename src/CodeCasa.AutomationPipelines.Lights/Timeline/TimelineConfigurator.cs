using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Timeline;

internal class TimelineConfigurator : ITimelineConfigurator
{
    private readonly List<(ITimeline Timeline, Func<IServiceProvider, LightParameters?> Factory)> _entries = [];
    internal TimeSpan? TransitionTime { get; private set; }

    internal Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> TimelineFactory =>
        sp =>
        {
            var result = new Dictionary<ITimeline, LightParameters>();
            foreach (var (timeline, factory) in _entries)
            {
                var parameters = factory(sp);
                if (parameters != null)
                    result[timeline] = parameters;
            }
            return result;
        };

    public ITimelineConfigurator Add(ITimeline timeline, LightParameters lightParameters)
    {
        _entries.Add((timeline, _ => lightParameters));
        return this;
    }

    public ITimelineConfigurator Add(ITimeline timeline, Func<IServiceProvider, LightParameters?> lightParametersFactory)
    {
        _entries.Add((timeline, lightParametersFactory));
        return this;
    }

    public ITimelineConfigurator SetTransitionTime(TimeSpan transitionTime)
    {
        TransitionTime = transitionTime;
        return this;
    }
}
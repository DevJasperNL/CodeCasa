using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightParameters));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, lightTransitions));
    }

    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, Action<ILightTransitionCycleConfigurator<TLight>> configure)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Cycle", LoggingEnabled ?? false)
            .AddCycle(triggerObservable, configure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
    {
        return AddCycle(triggerObservable, c => c.AddTimeline(timeline, transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return AddCycle(triggerObservable, c => c.AddTimeline(timelineFactory, transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Action<ITimelineConfigurator> configure)
    {
        return AddCycle(triggerObservable, c => c.AddTimeline(configure));
    }
}
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, lightParameters));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, lightParameters));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, lightTransitions));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, lightTransitions));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<Func<IServiceProvider, IPipelineNode<LightTransition>>> nodeFactories)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, nodeFactories));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params Func<IServiceProvider, IPipelineNode<LightTransition>>[] nodeFactories)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, nodeFactories));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionToggleConfigurator<TLight>> configure)
    {
        return AddReactiveNode(c => c
            .SetHierarchyContext(HierarchyPath, "Toggle", LoggingEnabled ?? false)
            .AddToggle(triggerObservable, configure));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
    {
        return AddToggle(triggerObservable, c => c.AddTimeline(timeline, transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
    {
        return AddToggle(triggerObservable, c => c.AddTimeline(timelineFactory, transitionTimeForTimelineState));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Action<ITimelineConfigurator> configure)
    {
        return AddToggle(triggerObservable, c => c.AddTimeline(configure));
    }
}
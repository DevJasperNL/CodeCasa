using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class CompositeLightTransitionReactiveNodeConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddToggle(triggerObservable, lightParameters.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var lightParameter in lightParameters)
            {
                configure.Add(lightParameter);
            }
        });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddToggle(triggerObservable, lightTransitions.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var lightTransition in lightTransitions)
            {
                configure.Add(lightTransition);
            }
        });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<Func<IServiceProvider, IPipelineNode<LightTransition>>> nodeFactories)
        => AddToggle(triggerObservable, nodeFactories.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, params Func<IServiceProvider, IPipelineNode<LightTransition>>[] nodeFactories)
    {
        return AddToggle(triggerObservable, configure =>
        {
            foreach (var fact in nodeFactories)
            {
                configure.Add(fact);
            }
        });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, Action<ILightTransitionToggleConfigurator<TLight>> configure)
    {
        var toggleConfigurators = configurators.ToDictionary(kvp => kvp.Key,
            kvp => new LightTransitionToggleConfigurator<TLight>(kvp.Value.Light, scheduler));
        var compositeToggleConfigurator = new CompositeLightTransitionToggleConfigurator<TLight>(toggleConfigurators, []);
        configure(compositeToggleConfigurator);
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);

        configurators.ForEach(kvp =>
        {
            var toggleConfig = toggleConfigurators[kvp.Key];
            var gracePeriod = toggleConfig.GracePeriod ?? TimeSpan.FromSeconds(1);
            kvp.Value.AddNodeSource(shareableTriggerObservable.ToToggleObservable(
                lastActivationTime =>
                {
                    var utcNow = DateTime.UtcNow;
                    if (utcNow - kvp.Value.Light.LastChangedUtc <= gracePeriod &&
                        (!lastActivationTime.HasValue || utcNow - lastActivationTime > gracePeriod))
                    {
                        return !configurators.Values.Any(c => c.Light.IsOn());
                    }

                    return configurators.Values.Any(c => c.Light.IsOn());
                },
                () => new TurnOffThenPassThroughNode(),
                toggleConfig.NodeFactories.Select(fact =>
                {
                    return new Func<IPipelineNode<LightTransition>>(() =>
                            fact.CreateScopedNode(kvp.Value
                                .ServiceProvider) // Note: This service provider already has the light registered. We scope it further for node lifetime.
                    );
                }),
                toggleConfig.ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
                toggleConfig.IncludeOffValue));
        });
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
        => AddToggle(triggerObservable, c => c.AddTimeline(timeline, transitionTimeForTimelineState));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
        => AddToggle(triggerObservable, c => c.AddTimeline(timelineFactory, transitionTimeForTimelineState));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Action<ITimelineConfigurator> configure)
        => AddToggle(triggerObservable, c => c.AddTimeline(configure));
}
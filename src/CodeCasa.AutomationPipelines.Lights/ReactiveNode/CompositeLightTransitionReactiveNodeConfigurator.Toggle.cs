using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Occurify;
using System.Reactive.Linq;

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

        var factoryCounts = toggleConfigurators.Values.Select(c => c.NodeFactories.Count).Distinct().ToArray();
        if (factoryCounts.Length == 1)
        {
            // The toggle step is determined once per trigger for all lights. Evaluating it per light let lights diverge,
            // because the first light's new node already changed the on/off state the next light was evaluated against.
            var firstConfig = toggleConfigurators.Values.First();
            var gracePeriod = firstConfig.GracePeriod ?? TimeSpan.FromSeconds(1);
            var shareableIndexObservable = _observableSharingStrategy.Apply(triggerObservable.ToToggleIndexObservable(
                lastActivationTime => IsToggleOff(lastActivationTime, gracePeriod),
                factoryCounts[0],
                firstConfig.ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
                firstConfig.IncludeOffValue,
                scheduler));

            configurators.ForEach(kvp =>
            {
                var nodeFactories = toggleConfigurators[kvp.Key].NodeFactories.ToArray();
                kvp.Value.AddNodeSource(shareableIndexObservable.Select(index => index == Extensions.ObservableExtensions.ToggleOffIndex
                    ? new TurnOffThenPassThroughNode()
                    : (IPipelineNode<LightTransition>?)nodeFactories[index].CreateScopedNode(kvp.Value.ServiceProvider)));
            });
            return this;
        }

        // ForLights without ExcludedLightBehaviours.PassThrough gives lights toggles of different lengths, so they can only toggle independently.
        var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
        configurators.ForEach(kvp =>
        {
            var toggleConfig = toggleConfigurators[kvp.Key];
            var gracePeriod = toggleConfig.GracePeriod ?? TimeSpan.FromSeconds(1);
            kvp.Value.AddNodeSource(shareableTriggerObservable.ToToggleObservable(
                lastActivationTime => IsToggleOff(lastActivationTime, gracePeriod),
                () => new TurnOffThenPassThroughNode(),
                toggleConfig.NodeFactories.Select(fact =>
                {
                    return new Func<IPipelineNode<LightTransition>>(() =>
                            fact.CreateScopedNode(kvp.Value
                                .ServiceProvider) // Note: This service provider already has the light registered. We scope it further for node lifetime.
                    );
                }),
                toggleConfig.ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
                toggleConfig.IncludeOffValue,
                scheduler));
        });
        return this;
    }

    private bool IsToggleOff(DateTime? lastActivationTime, TimeSpan gracePeriod)
    {
        var utcNow = scheduler.Now.UtcDateTime;
        var anyOn = configurators.Values.Any(c => c.Light.IsOn());
        // The most recent change of any light decides the grace period, so all lights take the same branch.
        var lastChangedUtc = configurators.Values.Max(c => c.Light.LastChangedUtc);
        if (utcNow - lastChangedUtc <= gracePeriod &&
            (!lastActivationTime.HasValue || utcNow - lastActivationTime > gracePeriod))
        {
            return !anyOn;
        }

        return anyOn;
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
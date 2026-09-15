using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using Occurify;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class CompositeLightTransitionReactiveNodeConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddCycle(triggerObservable, lightParameters.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddCycle(triggerObservable, configure =>
        {
            foreach (var lp in lightParameters)
            {
                configure.Add(lp);
            }
        });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddCycle(triggerObservable, lightTransitions.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddCycle(triggerObservable, configure =>
        {
            foreach (var lt in lightTransitions)
            {
                configure.Add(lt);
            }
        });
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable, Action<ILightTransitionCycleConfigurator<TLight>> configure)
    {
        var cycleConfigurators = configurators.ToDictionary(kvp => kvp.Key,
            kvp => new LightTransitionCycleConfigurator<TLight>(kvp.Value.Light));
        var compositeCycleConfigurator = new CompositeLightTransitionCycleConfigurator<TLight>(cycleConfigurators, []);
        configure(compositeCycleConfigurator);

        var entriesByLight = cycleConfigurators.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CycleNodeFactories.ToArray());
        var entryCounts = entriesByLight.Values.Select(entries => entries.Length).Distinct().ToArray();
        if (entryCounts.Length != 1)
        {
            // ForLights without ExcludedLightBehaviours.PassThrough gives lights cycles of different lengths, so they can only cycle independently.
            var shareableTriggerObservable = _observableSharingStrategy.Apply(triggerObservable);
            configurators.ForEach(kvp => kvp.Value.AddNodeSource(shareableTriggerObservable.ToCycleObservable(entriesByLight[kvp.Key].Select(tuple =>
            {
                var factory = new Func<IPipelineNode<LightTransition>>(() =>
                    tuple.nodeFactory.CreateScopedNode(kvp.Value.ServiceProvider) // Note: This service provider already has the light registered. We scope it further for node lifetime.
                    );
                var valueIsActiveFunc = () => tuple.matchesNodeState(kvp.Value.ServiceProvider);
                return (factory, valueIsActiveFunc);
            }))));
            return this;
        }

        var entryCount = entryCounts[0];
        if (entryCount == 0)
        {
            return this;
        }

        // The next entry is determined once per trigger for all lights. Evaluating it per light let lights end up on different
        // entries, because the first light's new node already changed the state the next light was matched against.
        var shareableIndexObservable = _observableSharingStrategy.Apply(triggerObservable.Select(_ => NextCycleIndex()));
        configurators.ForEach(kvp => kvp.Value.AddNodeSource(shareableIndexObservable.Select(index =>
            (IPipelineNode<LightTransition>?)entriesByLight[kvp.Key][index].nodeFactory.CreateScopedNode(kvp.Value.ServiceProvider))));
        return this;

        int NextCycleIndex()
        {
            var activeIndex = Enumerable.Range(0, entryCount).FirstOrDefault(
                i => configurators.All(kvp => entriesByLight[kvp.Key][i].matchesNodeState(kvp.Value.ServiceProvider)),
                -1);
            return (activeIndex + 1) % entryCount;
        }
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
        => AddCycle(triggerObservable, c => c.AddTimeline(timeline, transitionTimeForTimelineState));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, Dictionary<ITimeline, LightParameters>> timelineFactory,
        TimeSpan? transitionTimeForTimelineState = null)
        => AddCycle(triggerObservable, c => c.AddTimeline(timelineFactory, transitionTimeForTimelineState));

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Action<ITimelineConfigurator> configure)
        => AddCycle(triggerObservable, c => c.AddTimeline(configure));
}

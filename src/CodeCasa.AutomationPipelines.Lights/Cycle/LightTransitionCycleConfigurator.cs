using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Occurify;
using System.Reactive.Concurrency;
using Occurify.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class LightTransitionCycleConfigurator<TLight>(TLight light) : ILightTransitionCycleConfigurator<TLight>
    where TLight : ILight
{
    public TLight Light { get; } = light;

    internal List<(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory, Func<IServiceProvider, bool> matchesNodeState)> CycleNodeFactories
    {
        get;
    } = [];

    public ILightTransitionCycleConfigurator<TLight> AddOff()
    {
        return Add<TurnOffThenPassThroughNode>(_ => Light.IsOff());
    }

    public ILightTransitionCycleConfigurator<TLight> AddOn()
    {
        return Add(LightTransition.On());
    }

    public ILightTransitionCycleConfigurator<TLight> Add(LightParameters lightParameters, IEqualityComparer<LightParameters>? comparer = null)
    {
        return Add(lightParameters.AsTransition(), comparer);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, LightParameters?> lightParametersFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add(c => lightParametersFactory(c)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?, LightParameters?> lightParametersFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add((c, t) => lightParametersFactory(c, t)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(LightTransition lightTransition, IEqualityComparer<LightParameters>? comparer = null)
    {
        comparer ??= EqualityComparer<LightParameters>.Default;
        return Add(sp => new StaticLightTransitionNode(lightTransition, sp.GetRequiredService<IScheduler>()), _ => comparer.Equals(
            Light.GetParameters(),
            lightTransition.LightParameters));
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?> lightTransitionFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add(sp => new StaticLightTransitionNode(lightTransitionFactory(sp), sp.GetRequiredService<IScheduler>()), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?, LightTransition?> lightTransitionFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add(sp => new FactoryNode<LightTransition>(t => lightTransitionFactory(sp, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add<TNode>(Func<IServiceProvider, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        return Add(sp => ActivatorUtilities.CreateInstance<TNode>(sp), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        CycleNodeFactories.Add((nodeFactory, matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> AddPassThrough(Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add(_ => new PassThroughNode<LightTransition>(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> AddTimeline(Dictionary<ITimeline, LightParameters> timeline, TimeSpan? transitionTimeForTimelineState = null)
    {
        return Add(
            sp => new TimelineNode(timeline, sp.GetRequiredService<IScheduler>(), transitionTimeForTimelineState),
            _ => EqualityComparer<LightParameters>.Default.Equals(
                Light.GetParameters(),
                timeline.GetValuesAtCurrentOrNextUtcInstant(DateTime.UtcNow).Value.First()));
    }

    public ILightTransitionCycleConfigurator<TLight> AddTimeline(Action<ITimelineConfigurator> configure)
    {
        var configurator = new TimelineConfigurator();
        configure(configurator);
        return AddTimeline(configurator.Timeline, configurator.TransitionTime);
    }

    public ILightTransitionCycleConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([light], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<string> lightIds, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        CompositeHelper.ValidateLightSupported(lightIds, Light.Id);
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        CompositeHelper.ResolveGroupsAndValidateLightSupported(lights, Light.Id);
        return this;
    }
}
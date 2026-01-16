using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class LightTransitionCycleConfigurator<TLight>(TLight light, IScheduler scheduler) : ILightTransitionCycleConfigurator<TLight>
    where TLight : ILight
{
    public TLight Light { get; } = light;

    internal List<(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)> CycleNodeFactories
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

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightParameters?> lightParametersFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => lightParametersFactory(c)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightParameters?> lightParametersFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add((c, t) => lightParametersFactory(c, t)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(LightTransition lightTransition, IEqualityComparer<LightParameters>? comparer = null)
    {
        comparer ??= EqualityComparer<LightParameters>.Default;
        return Add(new StaticLightTransitionNode(lightTransition, scheduler), _ => comparer.Equals(
            Light.GetParameters(),
            lightTransition.LightParameters));
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), scheduler), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add<TNode>(Func<ILightPipelineContext<TLight>, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        return Add(c => c.ServiceProvider.CreateInstanceWithinContext<TNode, TLight>(c), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(IPipelineNode<LightTransition> node, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(_ => node, matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        CycleNodeFactories.Add((nodeFactory, matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> AddPassThrough(Func<ILightPipelineContext<TLight>, bool> matchesNodeState)
    {
        return Add(new PassThroughNode<LightTransition>(), matchesNodeState);
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
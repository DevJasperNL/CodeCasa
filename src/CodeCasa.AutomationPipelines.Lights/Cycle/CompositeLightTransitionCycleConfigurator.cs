using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class CompositeLightTransitionCycleConfigurator(
    Dictionary<string, LightTransitionCycleConfigurator> activeConfigurators, 
    Dictionary<string, LightTransitionCycleConfigurator> inactiveConfigurators)
    : ILightTransitionCycleConfigurator
{
    public ILightTransitionCycleConfigurator AddOff()
    {
        var matchesNodeState = () => activeConfigurators.Values.All(c => c.Light.IsOff());
        activeConfigurators.Values.ForEach(c => c.Add<TurnOffThenPassThroughNode>(_ => matchesNodeState()));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(_ => matchesNodeState()));
        return this;
    }

    public ILightTransitionCycleConfigurator AddOn()
    {
        return Add(LightTransition.On());
    }

    public ILightTransitionCycleConfigurator Add(LightParameters lightParameters, IEqualityComparer<LightParameters>? comparer = null)
    {
        return Add(lightParameters.AsTransition(), comparer);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightParameters?> lightParametersFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => lightParametersFactory(c)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightParameters?> lightParametersFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add((c, t) => lightParametersFactory(c, t)?.AsTransition(), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(LightTransition lightTransition, IEqualityComparer<LightParameters>? comparer = null)
    {
        comparer ??= EqualityComparer<LightParameters>.Default;
        return Add(
            _ => lightTransition,
            _ => activeConfigurators.Values.All(c => comparer.Equals(
                c.Light.GetParameters(),
                lightTransition.LightParameters)));
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.ServiceProvider.GetRequiredService<IScheduler>()), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, LightTransition?, LightTransition?> lightTransitionFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator Add<TNode>(Func<ILightPipelineContext, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        activeConfigurators.Values.ForEach(c => c.Add<TNode>(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator Add(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, Func<ILightPipelineContext, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.Add(nodeFactory, matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator AddPassThrough(Func<ILightPipelineContext, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator ForLight(string lightId, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator ForLight(ILight light, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([light], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator ForLights(IEnumerable<string> lightIds,
        Action<ILightTransitionCycleConfigurator> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        var lightIdArray =
            CompositeHelper.ValidateLightsSupported(lightIds, activeConfigurators.Keys);

        if (lightIdArray.Length == activeConfigurators.Count)
        {
            configure(this);
            return this;
        }

        if (excludedLightBehaviour == ExcludedLightBehaviours.None)
        {
            if (lightIdArray.Length == 1)
            {
                configure(activeConfigurators[lightIdArray.First()]);
                return this;
            }

            configure(new CompositeLightTransitionCycleConfigurator(
                activeConfigurators.Where(kvp => lightIdArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value), []));
            return this;
        }

        configure(new CompositeLightTransitionCycleConfigurator(
            activeConfigurators.Where(kvp => lightIdArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            activeConfigurators.Where(kvp => !lightIdArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        return this;
    }

    public ILightTransitionCycleConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionCycleConfigurator> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lightEntities, activeConfigurators.Keys);
        return ForLights(lightIds, configure, excludedLightBehaviour);
    }
}
using System.Reactive.Concurrency;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Cycle;

internal class CompositeLightTransitionCycleConfigurator<TLight>(
    Dictionary<string, LightTransitionCycleConfigurator<TLight>> activeConfigurators, 
    Dictionary<string, LightTransitionCycleConfigurator<TLight>> inactiveConfigurators)
    : ILightTransitionCycleConfigurator<TLight>
    where TLight : ILight
{
    public ILightTransitionCycleConfigurator<TLight> AddOff()
    {
        var matchesNodeState = () => activeConfigurators.Values.All(c => c.Light.IsOff());
        activeConfigurators.Values.ForEach(c => c.Add<TurnOffThenPassThroughNode>(_ => matchesNodeState()));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(_ => matchesNodeState()));
        return this;
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
        return Add(
            _ => lightTransition,
            _ => activeConfigurators.Values.All(c => comparer.Equals(
                c.Light.GetParameters(),
                lightTransition.LightParameters)));
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?> lightTransitionFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add(c => new StaticLightTransitionNode(lightTransitionFactory(c), c.GetRequiredService<IScheduler>()), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, LightTransition?, LightTransition?> lightTransitionFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        return Add(c => new FactoryNode<LightTransition>(t => lightTransitionFactory(c, t)), matchesNodeState);
    }

    public ILightTransitionCycleConfigurator<TLight> Add<TNode>(Func<IServiceProvider, bool> matchesNodeState) where TNode : IPipelineNode<LightTransition>
    {
        activeConfigurators.Values.ForEach(c => c.Add<TNode>(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> Add(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory, Func<IServiceProvider, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.Add(nodeFactory, matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> AddPassThrough(Func<IServiceProvider, bool> matchesNodeState)
    {
        activeConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        inactiveConfigurators.Values.ForEach(c => c.AddPassThrough(matchesNodeState));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([lightId], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None) => ForLights([light], configure, excludedLightBehaviour);

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<string> lightIds,
        Action<ILightTransitionCycleConfigurator<TLight>> configure,
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

            configure(new CompositeLightTransitionCycleConfigurator<TLight>(
                activeConfigurators.Where(kvp => lightIdArray.Contains(kvp.Key))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value), []));
            return this;
        }

        configure(new CompositeLightTransitionCycleConfigurator<TLight>(
            activeConfigurators.Where(kvp => lightIdArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            activeConfigurators.Where(kvp => !lightIdArray.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
        return this;
    }

    public ILightTransitionCycleConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionCycleConfigurator<TLight>> configure, ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        var lightIds = CompositeHelper.ResolveGroupsAndValidateLightsSupported(lights, activeConfigurators.Keys);
        return ForLights(lightIds, configure, excludedLightBehaviour);
    }
}
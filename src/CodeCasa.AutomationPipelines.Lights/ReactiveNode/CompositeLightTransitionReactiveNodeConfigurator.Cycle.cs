using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

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
            kvp => new LightTransitionCycleConfigurator<TLight>(kvp.Value.Light, scheduler));
        var compositeCycleConfigurator = new CompositeLightTransitionCycleConfigurator<TLight>(cycleConfigurators, []);
        configure(compositeCycleConfigurator);
        configurators.ForEach(kvp => kvp.Value.AddNodeSource(triggerObservable.ToCycleObservable(cycleConfigurators[kvp.Key].CycleNodeFactories.Select(tuple =>
        {
            var factory = new Func<IPipelineNode<LightTransition>>(() =>
            {
                var serviceScope = kvp.Value.ServiceProvider.CreateScope(); // Note: This service provider already has the light registered. We scope it further for node lifetime.
                var context = new LightPipelineContext<TLight>(serviceScope.ServiceProvider);
                return new ScopedNode<LightTransition>(serviceScope, tuple.nodeFactory(context));
            });
            var valueIsActiveFunc = () => tuple.matchesNodeState(new LightPipelineContext<TLight>(serviceProvider));
            return (factory, valueIsActiveFunc);
        }))));
        return this;
    }
}

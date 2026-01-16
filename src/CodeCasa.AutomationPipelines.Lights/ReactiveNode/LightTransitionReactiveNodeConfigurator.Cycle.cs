using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator<TLight>
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
    public ILightTransitionReactiveNodeConfigurator<TLight> AddCycle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionCycleConfigurator<TLight>> configure)
    {
        var cycleConfigurator = new LightTransitionCycleConfigurator<TLight>(Light, scheduler);
        configure(cycleConfigurator);
        AddNodeSource(triggerObservable.ToCycleObservable(cycleConfigurator.CycleNodeFactories.Select(tuple =>
        {
            var factory = new Func<IPipelineNode<LightTransition>>(() =>
            {
                var serviceScope = serviceProvider.CreateScope();
                var context = new LightPipelineContext<TLight>(serviceScope.ServiceProvider, Light);
                return new ScopedNode<LightTransition>(serviceScope, tuple.nodeFactory(context));
            });
            var valueIsActiveFunc = () => tuple.matchesNodeState(new LightPipelineContext<TLight>(serviceProvider, Light));
            return (factory, valueIsActiveFunc);
        })));
        return this;
    }
}

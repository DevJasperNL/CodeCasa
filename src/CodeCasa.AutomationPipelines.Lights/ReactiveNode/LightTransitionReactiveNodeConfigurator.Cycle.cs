using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddCycle(triggerObservable, lightParameters.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
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
    public ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddCycle(triggerObservable, lightTransitions.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
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
    public ILightTransitionReactiveNodeConfigurator AddCycle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionCycleConfigurator> configure)
    {
        var cycleConfigurator = new LightTransitionCycleConfigurator(Light, scheduler);
        configure(cycleConfigurator);
        AddNodeSource(triggerObservable.ToCycleObservable(cycleConfigurator.CycleNodeFactories.Select(tuple =>
        {
            var serviceScope = serviceProvider.CreateScope();
            var context = new LightPipelineContext(serviceScope.ServiceProvider, Light);
            var factory = new Func<IPipelineNode<LightTransition>>(() => new ScopedNode<LightTransition>(serviceScope, tuple.nodeFactory(context)));
            var valueIsActiveFunc = () => tuple.matchesNodeState(context);
            return (factory, valueIsActiveFunc);
        })));
        return this;
    }
}
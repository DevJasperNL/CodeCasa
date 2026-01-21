using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;

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
                var serviceScope = ServiceProvider.CreateLightContextScope(Light);
                return new ScopedNode<LightTransition>(serviceScope, tuple.nodeFactory(serviceScope.ServiceProvider));
            });
            var valueIsActiveFunc = () => tuple.matchesNodeState(ServiceProvider);
            return (factory, valueIsActiveFunc);
        })));
        return this;
    }
}

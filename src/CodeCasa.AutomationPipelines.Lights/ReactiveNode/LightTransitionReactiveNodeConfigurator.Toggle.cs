using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial class LightTransitionReactiveNodeConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddToggle(triggerObservable, lightParameters.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
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
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddToggle(triggerObservable, lightTransitions.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable,
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
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<Func<ILightPipelineContext, IPipelineNode<LightTransition>>> nodeFactories)
        => AddToggle(triggerObservable, nodeFactories.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable, params Func<ILightPipelineContext, IPipelineNode<LightTransition>>[] nodeFactories)
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
    public ILightTransitionReactiveNodeConfigurator AddToggle<T>(IObservable<T> triggerObservable, Action<ILightTransitionToggleConfigurator> configure)
    {
        var toggleConfigurator = new LightTransitionToggleConfigurator(LightEntity, scheduler);
        configure(toggleConfigurator);
        AddNodeSource(triggerObservable.ToToggleObservable(
            () => LightEntity.IsOn(),
            () => new TurnOffThenPassThroughNode(),
            toggleConfigurator.NodeFactories.Select(fact =>
            {
                return new Func<IPipelineNode<LightTransition>>(() =>
                {
                    var serviceScope = serviceProvider.CreateScope();
                    var context = new LightPipelineContext(serviceScope.ServiceProvider, LightEntity);
                    return new ScopedNode<LightTransition>(serviceScope, fact(context));
                });
            }),
            toggleConfigurator.ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
            toggleConfigurator.IncludeOffValue));
        return this;
    }
}
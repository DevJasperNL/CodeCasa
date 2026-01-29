using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightParameters> lightParameters)
        => AddToggle(triggerObservable, lightParameters.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
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
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<LightTransition> lightTransitions)
        => AddToggle(triggerObservable, lightTransitions.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
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
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, IEnumerable<Func<IServiceProvider, IPipelineNode<LightTransition>>> nodeFactories)
        => AddToggle(triggerObservable, nodeFactories.ToArray());

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, params Func<IServiceProvider, IPipelineNode<LightTransition>>[] nodeFactories)
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
    public ILightTransitionReactiveNodeConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable, Action<ILightTransitionToggleConfigurator<TLight>> configure)
    {
        var toggleConfigurator = new LightTransitionToggleConfigurator<TLight>(Light, _scheduler);
        configure(toggleConfigurator);
        AddNodeSource(triggerObservable.ToToggleObservable(
            () => Light.IsOn(),
            () => new TurnOffThenPassThroughNode(),
            toggleConfigurator.NodeFactories.Select(fact =>
            {
                return new Func<IPipelineNode<LightTransition>>(() => 
                    fact.CreateScopedNode(ServiceProvider) // Note: This service provider already has the light registered. We scope it further for node lifetime.
                    );
            }),
            toggleConfigurator.ToggleTimeout ?? TimeSpan.FromMilliseconds(1000),
            toggleConfigurator.IncludeOffValue));
        return this;
    }
}

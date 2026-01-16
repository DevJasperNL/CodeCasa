using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightParameters));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightParameters));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightTransitions));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightTransitions));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>>> nodeFactories)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, nodeFactories));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        params Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>>[] nodeFactories)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, nodeFactories));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> AddToggle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionToggleConfigurator<TLight>> configure)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, configure));
    }
}
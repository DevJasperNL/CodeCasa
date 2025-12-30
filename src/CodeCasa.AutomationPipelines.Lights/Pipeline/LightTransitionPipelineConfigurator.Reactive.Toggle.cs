using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial class LightTransitionPipelineConfigurator
{
    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightParameters> lightParameters)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightParameters));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightParameters[] lightParameters)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightParameters));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<LightTransition> lightTransitions)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightTransitions));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params LightTransition[] lightTransitions)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, lightTransitions));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        IEnumerable<Func<ILightPipelineContext, IPipelineNode<LightTransition>>> nodeFactories)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, nodeFactories));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        params Func<ILightPipelineContext, IPipelineNode<LightTransition>>[] nodeFactories)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, nodeFactories));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator AddToggle<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionToggleConfigurator> configure)
    {
        return AddReactiveNode(c => c.AddToggle(triggerObservable, configure));
    }
}
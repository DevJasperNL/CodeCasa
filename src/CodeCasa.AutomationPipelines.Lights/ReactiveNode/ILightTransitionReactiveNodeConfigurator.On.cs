using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public partial interface ILightTransitionReactiveNodeConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Registers a trigger that applies the given <paramref name="lightParameters"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the light parameter application.</param>
    /// <param name="lightParameters">The light parameters to apply when triggered.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        LightParameters lightParameters);

    /// <summary>
    /// Registers a trigger that applies light parameters created by <paramref name="lightParametersFactory"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the light parameter application.</param>
    /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, LightParameters> lightParametersFactory);

    /// <summary>
    /// Registers a trigger that applies the given <paramref name="lightTransition"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the light transition application.</param>
    /// <param name="lightTransition">The light transition to apply when triggered.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        LightTransition lightTransition);

    /// <summary>
    /// Registers a trigger that applies a light transition created by <paramref name="lightTransitionFactory"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the light transition application.</param>
    /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, LightTransition> lightTransitionFactory);

    /// <summary>
    /// Registers a trigger that activates a pipeline node of type <typeparamref name="TNode"/> when the <paramref name="triggerObservable"/> emits a value.
    /// The node is resolved from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <typeparam name="TNode">The type of the pipeline node to resolve and activate.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the node activation.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T, TNode>(IObservable<T> triggerObservable)
        where TNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a trigger that activates a pipeline node created by <paramref name="nodeFactory"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the node activation.</param>
    /// <param name="nodeFactory">A factory function that creates a pipeline node based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory);

    /// <summary>
    /// Registers a trigger that activates a nested pipeline configured by <paramref name="pipelineConfigurator"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the pipeline activation.</param>
    /// <param name="pipelineConfigurator">An action to configure the nested pipeline.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> pipelineConfigurator, 
        CompositeInstantiationScope instantiationScope = CompositeInstantiationScope.Shared);

    /// <summary>
    /// Registers a trigger that activates a nested reactive node configured by <paramref name="configure"/> when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the reactive node activation.</param>
    /// <param name="configure">An action to configure the nested reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> On<T>(IObservable<T> triggerObservable, 
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure,
        CompositeInstantiationScope instantiationScope = CompositeInstantiationScope.Shared);

    /// <summary>
    /// Registers a pass-through trigger that allows the current input to pass through unchanged when the <paramref name="triggerObservable"/> emits a value.
    /// This is useful for conditional logic where you want to maintain the current state under certain conditions.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers the pass-through behavior.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> PassThroughOn<T>(IObservable<T> triggerObservable);

    /// <summary>
    /// Registers a trigger that turns off the light when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers turning off the light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> TurnOffWhen<T>(IObservable<T> triggerObservable);

    /// <summary>
    /// Registers a trigger that turns on the light when the <paramref name="triggerObservable"/> emits a value.
    /// </summary>
    /// <typeparam name="T">The type of values emitted by the trigger observable.</typeparam>
    /// <param name="triggerObservable">The observable that triggers turning on the light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionReactiveNodeConfigurator<TLight> TurnOnWhen<T>(IObservable<T> triggerObservable);
}

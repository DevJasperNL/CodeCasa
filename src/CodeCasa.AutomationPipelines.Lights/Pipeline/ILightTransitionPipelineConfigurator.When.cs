using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator
{
    /// <summary>
    /// Registers a node that applies the given <paramref name="lightParameters"/> when the observable 
    /// of type <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="lightParameters">The light parameters to apply when the observable emits true.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TObservable>(LightParameters lightParameters)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies the given <paramref name="lightParameters"/> when the 
    /// <paramref name="observable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the light parameters.</param>
    /// <param name="lightParameters">The light parameters to apply when the observable emits true.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When(IObservable<bool> observable, LightParameters lightParameters);

    /// <summary>
    /// Registers a node that applies light parameters created by <paramref name="lightParametersFactory"/> 
    /// when the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, LightParameters> lightParametersFactory)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies light parameters created by <paramref name="lightParametersFactory"/> 
    /// when the <paramref name="observable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the light parameters.</param>
    /// <param name="lightParametersFactory">A factory function that creates light parameters based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, LightParameters> lightParametersFactory);

    /// <summary>
    /// Registers a node that applies the given <paramref name="lightTransition"/> when the observable 
    /// of type <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="lightTransition">The light transition to apply when the observable emits true.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TObservable>(LightTransition lightTransition)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies the given <paramref name="lightTransition"/> when the 
    /// <paramref name="observable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the light transition.</param>
    /// <param name="lightTransition">The light transition to apply when the observable emits true.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When(IObservable<bool> observable, LightTransition lightTransition);

    /// <summary>
    /// Registers a node that applies the <see cref="LightTransition"/> created by 
    /// <paramref name="lightTransitionFactory"/> when the observable of type 
    /// <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies the <see cref="LightTransition"/> created by 
    /// <paramref name="lightTransitionFactory"/> when the <paramref name="observable"/> 
    /// emits <see langword="true"/>. When the observable emits <see langword="false"/>, 
    /// inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the light transition.</param>
    /// <param name="lightTransitionFactory">A factory function that creates a light transition based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, LightTransition> lightTransitionFactory);

    /// <summary>
    /// Registers a node created by <paramref name="nodeFactory"/> when the observable of type 
    /// <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="nodeFactory">A factory function that creates a pipeline node based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TObservable>(
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node created by <paramref name="nodeFactory"/> when the <paramref name="observable"/> 
    /// emits <see langword="true"/>. When the observable emits <see langword="false"/>, 
    /// inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the node.</param>
    /// <param name="nodeFactory">A factory function that creates a pipeline node based on the pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When(IObservable<bool> observable,
        Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory);

    /// <summary>
    /// Registers a node of type <typeparamref name="TNode"/> when the observable of type 
    /// <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// Both the observable and the node are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TNode">The type of the pipeline node to resolve from the service provider.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TObservable, TNode>()
        where TObservable : IObservable<bool>
        where TNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a node of type <typeparamref name="TNode"/> when the <paramref name="observable"/> 
    /// emits <see langword="true"/>. When the observable emits <see langword="false"/>, 
    /// inputs are passed through unchanged. The node is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TNode">The type of the pipeline node to resolve from the service provider.</typeparam>
    /// <param name="observable">The observable that determines when to apply the node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator When<TNode>(IObservable<bool> observable)
        where TNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a reactive node configured by <paramref name="configure"/> when the observable of type 
    /// <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="configure">An action to configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddReactiveNodeWhen<TObservable>(
        Action<ILightTransitionReactiveNodeConfigurator> configure)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a reactive node configured by <paramref name="configure"/> when the <paramref name="observable"/> 
    /// emits <see langword="true"/>. When the observable emits <see langword="false"/>, 
    /// inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the reactive node.</param>
    /// <param name="configure">An action to configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddReactiveNodeWhen(IObservable<bool> observable,
        Action<ILightTransitionReactiveNodeConfigurator> configure);

    /// <summary>
    /// Registers a pipeline configured by <paramref name="configure"/> when the observable of type 
    /// <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="configure">An action to configure the nested pipeline.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddPipelineWhen<TObservable>(
        Action<ILightTransitionPipelineConfigurator> configure)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a pipeline configured by <paramref name="configure"/> when the <paramref name="observable"/> 
    /// emits <see langword="true"/>. When the observable emits <see langword="false"/>, 
    /// inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to apply the pipeline.</param>
    /// <param name="configure">An action to configure the nested pipeline.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddPipelineWhen(IObservable<bool> observable,
        Action<ILightTransitionPipelineConfigurator> configure);

    /// <summary>
    /// Registers a node that turns off the light when the observable 
    /// of type <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator TurnOffWhen<TObservable>() where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that turns off the light when the <paramref name="observable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to turn off the light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator TurnOffWhen(IObservable<bool> observable);

    /// <summary>
    /// Registers a node that turns on the light when the observable 
    /// of type <typeparamref name="TObservable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged. 
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator TurnOnWhen<TObservable>() where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that turns on the light when the <paramref name="observable"/> emits <see langword="true"/>. 
    /// When the observable emits <see langword="false"/>, inputs are passed through unchanged.
    /// </summary>
    /// <param name="observable">The observable that determines when to turn on the light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator TurnOnWhen(IObservable<bool> observable);
}
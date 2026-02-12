using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Registers a node that switches between two sets of light parameters based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, 
    /// the <paramref name="trueLightParameters"/> are applied; when it emits <see langword="false"/>, 
    /// the <paramref name="falseLightParameters"/> are applied.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightParameters">The light parameters to apply when the observable emits true.</param>
    /// <param name="falseLightParameters">The light parameters to apply when the observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightParameters trueLightParameters,
        LightParameters falseLightParameters)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that switches between two sets of light parameters based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the <paramref name="trueLightParameters"/> are applied; 
    /// when it emits <see langword="false"/>, the <paramref name="falseLightParameters"/> are applied.
    /// </summary>
    /// <param name="observable">The observable that determines which parameters to apply.</param>
    /// <param name="trueLightParameters">The light parameters to apply when the observable emits true.</param>
    /// <param name="falseLightParameters">The light parameters to apply when the observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        LightParameters trueLightParameters, LightParameters falseLightParameters);

    /// <summary>
    /// Registers a node that switches between two sets of light parameters created by factory functions based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, 
    /// the parameters from <paramref name="trueLightParametersFactory"/> are applied; when it emits <see langword="false"/>, 
    /// the parameters from <paramref name="falseLightParametersFactory"/> are applied.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightParametersFactory">A factory function that creates light parameters for true values.</param>
    /// <param name="falseLightParametersFactory">A factory function that creates light parameters for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that switches between two sets of light parameters created by factory functions based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the parameters from <paramref name="trueLightParametersFactory"/> are applied; 
    /// when it emits <see langword="false"/>, the parameters from <paramref name="falseLightParametersFactory"/> are applied.
    /// </summary>
    /// <param name="observable">The observable that determines which parameters to apply.</param>
    /// <param name="trueLightParametersFactory">A factory function that creates light parameters for true values.</param>
    /// <param name="falseLightParametersFactory">A factory function that creates light parameters for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory);

    /// <summary>
    /// Registers a node that switches between two light transitions based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, 
    /// the <paramref name="trueLightTransition"/> is applied; when it emits <see langword="false"/>, 
    /// the <paramref name="falseLightTransition"/> is applied.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightTransition">The light transition to apply when the observable emits true.</param>
    /// <param name="falseLightTransition">The light transition to apply when the observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(LightTransition trueLightTransition,
        LightTransition falseLightTransition)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that switches between two light transitions based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the <paramref name="trueLightTransition"/> is applied; 
    /// when it emits <see langword="false"/>, the <paramref name="falseLightTransition"/> is applied.
    /// </summary>
    /// <param name="observable">The observable that determines which transition to apply.</param>
    /// <param name="trueLightTransition">The light transition to apply when the observable emits true.</param>
    /// <param name="falseLightTransition">The light transition to apply when the observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        LightTransition trueLightTransition, LightTransition falseLightTransition);

    /// <summary>
    /// Registers a node that switches between two light transitions created by factory functions based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, 
    /// the transition from <paramref name="trueLightTransitionFactory"/> is applied; when it emits <see langword="false"/>, 
    /// the transition from <paramref name="falseLightTransitionFactory"/> is applied.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightTransitionFactory">A factory function that creates a light transition for true values.</param>
    /// <param name="falseLightTransitionFactory">A factory function that creates a light transition for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that switches between two light transitions created by factory functions based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the transition from <paramref name="trueLightTransitionFactory"/> is applied; 
    /// when it emits <see langword="false"/>, the transition from <paramref name="falseLightTransitionFactory"/> is applied.
    /// </summary>
    /// <param name="observable">The observable that determines which transition to apply.</param>
    /// <param name="trueLightTransitionFactory">A factory function that creates a light transition for true values.</param>
    /// <param name="falseLightTransitionFactory">A factory function that creates a light transition for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory);

    /// <summary>
    /// Registers a node that switches between two pipeline nodes created by factory functions based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, 
    /// the node from <paramref name="trueNodeFactory"/> is applied; when it emits <see langword="false"/>, 
    /// the node from <paramref name="falseNodeFactory"/> is applied.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueNodeFactory">A factory function that creates a pipeline node for true values.</param>
    /// <param name="falseNodeFactory">A factory function that creates a pipeline node for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that switches between two pipeline nodes created by factory functions based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the node from <paramref name="trueNodeFactory"/> is applied; 
    /// when it emits <see langword="false"/>, the node from <paramref name="falseNodeFactory"/> is applied.
    /// </summary>
    /// <param name="observable">The observable that determines which node to apply.</param>
    /// <param name="trueNodeFactory">A factory function that creates a pipeline node for true values.</param>
    /// <param name="falseNodeFactory">A factory function that creates a pipeline node for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch(IObservable<bool> observable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory);

    /// <summary>
    /// Registers a node that switches between two pipeline node types based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>, 
    /// a node of type <typeparamref name="TTrueNode"/> is applied; when it emits <see langword="false"/>, 
    /// a node of type <typeparamref name="TFalseNode"/> is applied.
    /// The observable and both node types are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TTrueNode">The type of the pipeline node to apply for true values.</typeparam>
    /// <typeparam name="TFalseNode">The type of the pipeline node to apply for false values.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TObservable, TTrueNode, TFalseNode>()
        where TObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a node that switches between two pipeline node types based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, a node of type <typeparamref name="TTrueNode"/> is applied; 
    /// when it emits <see langword="false"/>, a node of type <typeparamref name="TFalseNode"/> is applied.
    /// Both node types are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TTrueNode">The type of the pipeline node to apply for true values.</typeparam>
    /// <typeparam name="TFalseNode">The type of the pipeline node to apply for false values.</typeparam>
    /// <param name="observable">The observable that determines which node to apply.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> Switch<TTrueNode, TFalseNode>(IObservable<bool> observable)
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a reactive node that switches between two configurations based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>,
    /// the node is configured by <paramref name="trueConfigure"/>; when it emits <see langword="false"/>,
    /// the node is configured by <paramref name="falseConfigure"/>.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueConfigure">An action to configure the reactive node for true values.</param>
    /// <param name="falseConfigure">An action to configure the reactive node for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the reactive node. Use <see cref="InstantiationScope.Shared"/> to create the node once and reuse it for all switches (lifetime matches the parent pipeline), or <see cref="InstantiationScope.PerChild"/> to create a new node each time the observable emits true (disposed when replaced).
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch<TObservable>(
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a reactive node that switches between two configurations based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the node is configured by <paramref name="trueConfigure"/>; 
    /// when it emits <see langword="false"/>, the node is configured by <paramref name="falseConfigure"/>.
    /// </summary>
    /// <param name="observable">The observable that determines which configuration to apply.</param>
    /// <param name="trueConfigure">An action to configure the reactive node for true values.</param>
    /// <param name="falseConfigure">An action to configure the reactive node for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the reactive node. Use <see cref="InstantiationScope.Shared"/> to create the node once and reuse it for all switches (lifetime matches the parent pipeline), or <see cref="InstantiationScope.PerChild"/> to create a new node each time the observable emits true (disposed when replaced).
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitch(IObservable<bool> observable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared);

    /// <summary>
    /// Registers a pipeline that switches between two configurations based on a boolean observable.
    /// When the observable of type <typeparamref name="TObservable"/> emits <see langword="true"/>,
    /// the pipeline is configured by <paramref name="trueConfigure"/>; when it emits <see langword="false"/>,
    /// the pipeline is configured by <paramref name="falseConfigure"/>.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <param name="trueConfigure">An action to configure the pipeline for true values.</param>
    /// <param name="falseConfigure">An action to configure the pipeline for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the pipeline. Use <see cref="InstantiationScope.Shared"/> to create the pipeline once and reuse it for all switches (lifetime matches the parent pipeline), or <see cref="InstantiationScope.PerChild"/> to create a new pipeline each time the observable emits true (disposed when replaced).
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch<TObservable>(
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a pipeline that switches between two configurations based on the <paramref name="observable"/>.
    /// When the observable emits <see langword="true"/>, the pipeline is configured by <paramref name="trueConfigure"/>; 
    /// when it emits <see langword="false"/>, the pipeline is configured by <paramref name="falseConfigure"/>.
    /// </summary>
    /// <param name="observable">The observable that determines which configuration to apply.</param>
    /// <param name="trueConfigure">An action to configure the pipeline for true values.</param>
    /// <param name="falseConfigure">An action to configure the pipeline for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the pipeline. Use <see cref="InstantiationScope.Shared"/> to create the pipeline once and reuse it for all switches (lifetime matches the parent pipeline), or <see cref="InstantiationScope.PerChild"/> to create a new pipeline each time the observable emits true (disposed when replaced).
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitch(IObservable<bool> observable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared);

    /// <summary>
    /// Registers a node that turns the light on when the observable of type <typeparamref name="TObservable"/> 
    /// emits <see langword="true"/>, and turns it off when it emits <see langword="false"/>.
    /// The observable is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TObservable">The type of the observable to resolve from the service provider.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> TurnOnOff<TObservable>() where TObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that turns the light on when the <paramref name="observable"/> emits <see langword="true"/>, 
    /// and turns it off when it emits <see langword="false"/>.
    /// </summary>
    /// <param name="observable">The observable that determines whether to turn the light on or off.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> TurnOnOff(IObservable<bool> observable);
}

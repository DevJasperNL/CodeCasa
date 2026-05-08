using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.AutomationPipelines.Lights.Switch;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Registers a node that applies a switch between two sets of light parameters when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which branch to apply: <paramref name="trueLightParameters"/> when <see langword="true"/>,
    /// <paramref name="falseLightParameters"/> when <see langword="false"/>.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueLightParameters">The light parameters to apply when the switch observable emits true.</param>
    /// <param name="falseLightParameters">The light parameters to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        LightParameters trueLightParameters, LightParameters falseLightParameters);

    /// <summary>
    /// Registers a node that applies a switch between two sets of light parameters when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> determines which branch to apply.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightParameters">The light parameters to apply when the switch observable emits true.</param>
    /// <param name="falseLightParameters">The light parameters to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        LightParameters trueLightParameters, LightParameters falseLightParameters)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two sets of light parameters when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> determines which branch to apply and is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueLightParameters">The light parameters to apply when the switch observable emits true.</param>
    /// <param name="falseLightParameters">The light parameters to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        LightParameters trueLightParameters, LightParameters falseLightParameters)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two sets of light parameters created by factory functions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which branch to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueLightParametersFactory">A factory function that creates light parameters for true values.</param>
    /// <param name="falseLightParametersFactory">A factory function that creates light parameters for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory);

    /// <summary>
    /// Registers a node that applies a switch between two sets of light parameters created by factory functions when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightParametersFactory">A factory function that creates light parameters for true values.</param>
    /// <param name="falseLightParametersFactory">A factory function that creates light parameters for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two sets of light parameters created by factory functions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueLightParametersFactory">A factory function that creates light parameters for true values.</param>
    /// <param name="falseLightParametersFactory">A factory function that creates light parameters for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Func<IServiceProvider, LightParameters> trueLightParametersFactory,
        Func<IServiceProvider, LightParameters> falseLightParametersFactory)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two light transitions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which transition to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueLightTransition">The light transition to apply when the switch observable emits true.</param>
    /// <param name="falseLightTransition">The light transition to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        LightTransition trueLightTransition, LightTransition falseLightTransition);

    /// <summary>
    /// Registers a node that applies a switch between two light transitions when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightTransition">The light transition to apply when the switch observable emits true.</param>
    /// <param name="falseLightTransition">The light transition to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        LightTransition trueLightTransition, LightTransition falseLightTransition)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two light transitions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueLightTransition">The light transition to apply when the switch observable emits true.</param>
    /// <param name="falseLightTransition">The light transition to apply when the switch observable emits false.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        LightTransition trueLightTransition, LightTransition falseLightTransition)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two light transitions created by factory functions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which branch to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueLightTransitionFactory">A factory function that creates a light transition for true values.</param>
    /// <param name="falseLightTransitionFactory">A factory function that creates a light transition for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory);

    /// <summary>
    /// Registers a node that applies a switch between two light transitions created by factory functions when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueLightTransitionFactory">A factory function that creates a light transition for true values.</param>
    /// <param name="falseLightTransitionFactory">A factory function that creates a light transition for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two light transitions created by factory functions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueLightTransitionFactory">A factory function that creates a light transition for true values.</param>
    /// <param name="falseLightTransitionFactory">A factory function that creates a light transition for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Func<IServiceProvider, LightTransition> trueLightTransitionFactory,
        Func<IServiceProvider, LightTransition> falseLightTransitionFactory)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two pipeline nodes created by factory functions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which branch to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueNodeFactory">A factory function that creates a pipeline node for true values.</param>
    /// <param name="falseNodeFactory">A factory function that creates a pipeline node for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory);

    /// <summary>
    /// Registers a node that applies a switch between two pipeline nodes created by factory functions when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueNodeFactory">A factory function that creates a pipeline node for true values.</param>
    /// <param name="falseNodeFactory">A factory function that creates a pipeline node for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two pipeline nodes created by factory functions when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueNodeFactory">A factory function that creates a pipeline node for true values.</param>
    /// <param name="falseNodeFactory">A factory function that creates a pipeline node for false values.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Func<IServiceProvider, IPipelineNode<LightTransition>> trueNodeFactory,
        Func<IServiceProvider, IPipelineNode<LightTransition>> falseNodeFactory)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that applies a switch between two pipeline node types when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// All types are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TTrueNode">The type of the pipeline node to apply for true values.</typeparam>
    /// <typeparam name="TFalseNode">The type of the pipeline node to apply for false values.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable, TTrueNode, TFalseNode>()
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a node that applies a switch between two pipeline node types when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> and both node types are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TTrueNode">The type of the pipeline node to apply for true values.</typeparam>
    /// <typeparam name="TFalseNode">The type of the pipeline node to apply for false values.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable, TTrueNode, TFalseNode>(
        IObservable<bool> whenObservable)
        where TSwitchObservable : IObservable<bool>
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a node that applies a switch between two pipeline node types when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both node types are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TTrueNode">The type of the pipeline node to apply for true values.</typeparam>
    /// <typeparam name="TFalseNode">The type of the pipeline node to apply for false values.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TTrueNode, TFalseNode>(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable)
        where TTrueNode : IPipelineNode<LightTransition>
        where TFalseNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Registers a reactive node switch configured via a fluent <see cref="ILightTransitionSwitchConfigurator{TLight}"/> when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which branch to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="configure">An action that receives the switch configurator to define both branches.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable, Action<ILightTransitionSwitchConfigurator<TLight>> configure);

    /// <summary>
    /// Registers a reactive node switch configured via a fluent <see cref="ILightTransitionSwitchConfigurator{TLight}"/> when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="configure">An action that receives the switch configurator to define both branches.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a reactive node switch configured via a fluent <see cref="ILightTransitionSwitchConfigurator{TLight}"/> when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="configure">An action that receives the switch configurator to define both branches.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> SwitchWhen<TSwitchObservable>(IObservable<bool> whenObservable,
        Action<ILightTransitionSwitchConfigurator<TLight>> configure)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a reactive node that switches between two configurations when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which configuration to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueConfigure">An action to configure the reactive node for true values.</param>
    /// <param name="falseConfigure">An action to configure the reactive node for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the reactive node.
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared);

    /// <summary>
    /// Registers a reactive node that switches between two configurations when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueConfigure">An action to configure the reactive node for true values.</param>
    /// <param name="falseConfigure">An action to configure the reactive node for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the reactive node.
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a reactive node that switches between two configurations when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueConfigure">An action to configure the reactive node for true values.</param>
    /// <param name="falseConfigure">An action to configure the reactive node for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the reactive node.
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddReactiveNodeSwitchWhen<TSwitchObservable>(
        IObservable<bool> whenObservable,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a pipeline that switches between two configurations when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The <paramref name="switchObservable"/> determines which configuration to apply.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that selects which branch to apply.</param>
    /// <param name="trueConfigure">An action to configure the pipeline for true values.</param>
    /// <param name="falseConfigure">An action to configure the pipeline for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the pipeline.
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared);

    /// <summary>
    /// Registers a pipeline that switches between two configurations when the observable of type
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="trueConfigure">An action to configure the pipeline for true values.</param>
    /// <param name="falseConfigure">An action to configure the pipeline for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the pipeline.
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen<TWhenObservable, TSwitchObservable>(
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a pipeline that switches between two configurations when the
    /// <paramref name="whenObservable"/> emits <see langword="true"/>, and passes inputs through unchanged when it emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="trueConfigure">An action to configure the pipeline for true values.</param>
    /// <param name="falseConfigure">An action to configure the pipeline for false values.</param>
    /// <param name="instantiationScope">
    /// Specifies the instantiation scope for the pipeline.
    /// </param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddPipelineSwitchWhen<TSwitchObservable>(
        IObservable<bool> whenObservable,
        Action<ILightTransitionPipelineConfigurator<TLight>> trueConfigure,
        Action<ILightTransitionPipelineConfigurator<TLight>> falseConfigure,
        InstantiationScope instantiationScope = InstantiationScope.Shared)
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that turns on the light when the <paramref name="whenObservable"/> emits <see langword="true"/> and 
    /// the <paramref name="switchObservable"/> emits <see langword="true"/>, turns off the light when the <paramref name="whenObservable"/> 
    /// emits <see langword="true"/> and the <paramref name="switchObservable"/> emits <see langword="false"/>, 
    /// and passes inputs through unchanged when the <paramref name="whenObservable"/> emits <see langword="false"/>.
    /// </summary>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <param name="switchObservable">The observable that determines whether to turn on or off the light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen(IObservable<bool> whenObservable,
        IObservable<bool> switchObservable);

    /// <summary>
    /// Registers a node that turns on the light when the observable of type <typeparamref name="TWhenObservable"/> emits <see langword="true"/> and 
    /// the observable of type <typeparamref name="TSwitchObservable"/> emits <see langword="true"/>, turns off the light when 
    /// <typeparamref name="TWhenObservable"/> emits <see langword="true"/> and <typeparamref name="TSwitchObservable"/> emits <see langword="false"/>, 
    /// and passes inputs through unchanged when <typeparamref name="TWhenObservable"/> emits <see langword="false"/>.
    /// Both observables are resolved from the service provider.
    /// </summary>
    /// <typeparam name="TWhenObservable">The type of the gating observable to resolve from the service provider.</typeparam>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen<TWhenObservable, TSwitchObservable>()
        where TWhenObservable : IObservable<bool>
        where TSwitchObservable : IObservable<bool>;

    /// <summary>
    /// Registers a node that turns on the light when the <paramref name="whenObservable"/> emits <see langword="true"/> and 
    /// the observable of type <typeparamref name="TSwitchObservable"/> emits <see langword="true"/>, turns off the light when 
    /// <paramref name="whenObservable"/> emits <see langword="true"/> and <typeparamref name="TSwitchObservable"/> emits <see langword="false"/>, 
    /// and passes inputs through unchanged when <paramref name="whenObservable"/> emits <see langword="false"/>.
    /// The observable of type <typeparamref name="TSwitchObservable"/> is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TSwitchObservable">The type of the branch-selecting observable to resolve from the service provider.</typeparam>
    /// <param name="whenObservable">The observable that gates the switch; when false, inputs pass through unchanged.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> TurnOnOffWhen<TSwitchObservable>(IObservable<bool> whenObservable)
        where TSwitchObservable : IObservable<bool>;
}
using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

/// <summary>
/// Configures a light transition pipeline for controlling light behavior through layered logic nodes.
/// Supports adding nodes, reactive nodes, dimmers, and conditional logic with When/Switch operators.
/// </summary>
/// <typeparam name="TLight">The specific type of light being controlled, which must implement <see cref="ILight"/>.</typeparam>
public partial interface ILightTransitionPipelineConfigurator<TLight> where TLight : ILight
{
    /// <summary>
    /// Enables logging for the pipeline configuration.
    /// </summary>
    /// <param name="pipelineName">The optional name of the pipeline to include in logs.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> EnableLogging(string? pipelineName = null);

    /// <summary>
    /// Disables logging for the pipeline configuration.
    /// </summary>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> DisableLogging();

    /// <summary>
    /// Adds a pipeline node of type <typeparamref name="TNode"/> to the pipeline.
    /// The node is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TNode">The type of the pipeline node to add.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddNode<TNode>() where TNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Adds a pipeline node created by the specified <paramref name="nodeFactory"/> to the pipeline.
    /// </summary>
    /// <param name="nodeFactory">A factory function that creates a pipeline node based on the light pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddNode(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory);

    /// <summary>
    /// Adds a reactive node to the pipeline configured by the specified <paramref name="configure"/> action.
    /// </summary>
    /// <param name="configure">An action to configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddReactiveNode(Action<ILightTransitionReactiveNodeConfigurator<TLight>> configure);

    /// <summary>
    /// Adds a nested pipeline to the current pipeline configured by the specified <paramref name="pipelineNodeOptions"/> action.
    /// </summary>
    /// <param name="pipelineNodeOptions">An action to configure the nested pipeline.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddPipeline(Action<ILightTransitionPipelineConfigurator<TLight>> pipelineNodeOptions);

    /// <summary>
    /// Adds a dimmer control to the pipeline.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddDimmer(IDimmer dimmer);

    /// <summary>
    /// Adds a dimmer control to the pipeline with custom configuration options.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add.</param>
    /// <param name="dimOptions">An action to configure the dimmer options.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> AddDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions);

    /// <summary>
    /// Creates a scoped pipeline configuration for a specific light identified by its entity ID.
    /// </summary>
    /// <param name="lightId">The entity ID of the light to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> ForLight(string lightId, Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder);

    /// <summary>
    /// Creates a scoped pipeline configuration for a specific light.
    /// </summary>
    /// <param name="light">The light to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> ForLight(TLight light, Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder);

    /// <summary>
    /// Creates a scoped pipeline configuration for multiple light entities identified by their entity IDs.
    /// </summary>
    /// <param name="lightIds">The entity IDs of the lights to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> ForLights(IEnumerable<string> lightIds, Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder);

    /// <summary>
    /// Creates a scoped pipeline configuration for multiple light entities.
    /// </summary>
    /// <param name="lights">The light entities to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> ForLights(IEnumerable<TLight> lights, Action<ILightTransitionPipelineConfigurator<TLight>> compositeNodeBuilder);
}

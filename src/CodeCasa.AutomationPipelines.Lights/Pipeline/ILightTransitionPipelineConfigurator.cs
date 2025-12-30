using CodeCasa.Abstractions;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator
{
    /// <summary>
    /// Sets the name for the current pipeline configuration.
    /// </summary>
    /// <param name="name">The name to assign to the pipeline.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator SetName(string name);

    /// <summary>
    /// Adds a pipeline node of type <typeparamref name="TNode"/> to the pipeline.
    /// The node is resolved from the service provider.
    /// </summary>
    /// <typeparam name="TNode">The type of the pipeline node to add.</typeparam>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddNode<TNode>() where TNode : IPipelineNode<LightTransition>;

    /// <summary>
    /// Adds a pipeline node created by the specified <paramref name="nodeFactory"/> to the pipeline.
    /// </summary>
    /// <param name="nodeFactory">A factory function that creates a pipeline node based on the light pipeline context.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddNode(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory);

    /// <summary>
    /// Adds a reactive node to the pipeline configured by the specified <paramref name="configure"/> action.
    /// </summary>
    /// <param name="configure">An action to configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddReactiveNode(Action<ILightTransitionReactiveNodeConfigurator> configure);

    /// <summary>
    /// Adds a nested pipeline to the current pipeline configured by the specified <paramref name="pipelineNodeOptions"/> action.
    /// </summary>
    /// <param name="pipelineNodeOptions">An action to configure the nested pipeline.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddPipeline(Action<ILightTransitionPipelineConfigurator> pipelineNodeOptions);

    /// <summary>
    /// Adds a dimmer control to the pipeline.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer);

    /// <summary>
    /// Adds a dimmer control to the pipeline with custom configuration options.
    /// </summary>
    /// <param name="dimmer">The dimmer device to add.</param>
    /// <param name="dimOptions">An action to configure the dimmer options.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator AddDimmer(IDimmer dimmer, Action<DimmerOptions> dimOptions);

    /// <summary>
    /// Creates a scoped pipeline configuration for a specific light identified by its entity ID.
    /// </summary>
    /// <param name="lightId">The entity ID of the light to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator ForLight(string lightId, Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder);

    /// <summary>
    /// Creates a scoped pipeline configuration for a specific light.
    /// </summary>
    /// <param name="light">The light to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator ForLight(ILight light, Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder);

    /// <summary>
    /// Creates a scoped pipeline configuration for multiple light entities identified by their entity IDs.
    /// </summary>
    /// <param name="lightIds">The entity IDs of the lights to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator ForLights(IEnumerable<string> lightIds, Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder);

    /// <summary>
    /// Creates a scoped pipeline configuration for multiple light entities.
    /// </summary>
    /// <param name="lightEntities">The light entities to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator ForLights(IEnumerable<ILight> lightEntities, Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder);
}
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

/// <summary>
/// Extension methods for <see cref="ILightTransitionPipelineConfigurator{TLight}"/>.
/// </summary>
public static class LightTransitionPipelineConfiguratorExtensions
{
    /// <summary>
    /// Adds a reactive node to the pipeline that handles external light state changes,
    /// with additional configuration applied via <paramref name="configure"/>.
    /// </summary>
    /// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="configure">An optional action to further configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<TLight> AddInteractionNode<TLight>(
        this ILightTransitionPipelineConfigurator<TLight> configurator, Action<ILightTransitionReactiveNodeConfigurator<TLight>>? configure = null) where TLight : ILight
    {
        return configurator.AddInteractionNode(new InteractionNodeOptions(), configure);
    }

    /// <summary>
    /// Adds a reactive node to the pipeline that handles external light state changes as described by <paramref name="options"/>,
    /// with additional configuration applied via <paramref name="configure"/>.
    /// </summary>
    /// <remarks>
    /// Turning the light off externally is always handled: the light stays off until the pipeline output upstream of this node
    /// changes. With <see cref="InteractionNodeOptions.HoldExternalChanges"/> other external changes are held as well.
    /// </remarks>
    /// <typeparam name="TLight">The specific type of light being controlled.</typeparam>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="options">The options controlling which external changes are handled and for how long.</param>
    /// <param name="configure">An optional action to further configure the reactive node.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<TLight> AddInteractionNode<TLight>(
        this ILightTransitionPipelineConfigurator<TLight> configurator, InteractionNodeOptions options,
        Action<ILightTransitionReactiveNodeConfigurator<TLight>>? configure = null) where TLight : ILight
    {
        return configurator.AddReactiveNode(c =>
        {
            configure?.Invoke(c);
            c.HandleExternalLightStateChanges(options);
        });
    }
}

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
        return configurator.AddReactiveNode(c =>
        {
            configure?.Invoke(c);
            c.HandleExternalLightStateChanges();
        });
    }
}

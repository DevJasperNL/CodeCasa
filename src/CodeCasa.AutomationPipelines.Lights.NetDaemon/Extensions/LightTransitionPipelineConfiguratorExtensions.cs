using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for light pipeline configurators to work with NetDaemon light entities.
/// </summary>
public static class LightTransitionPipelineConfiguratorExtensions
{
    /// <summary>
    /// Creates a scoped pipeline configuration for a specific NetDaemon light entity.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="lightEntity">The NetDaemon light entity to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator ForLight(this ILightTransitionPipelineConfigurator configurator,
        ILightEntityCore lightEntity, Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
    {
        return configurator.ForLight(lightEntity.AsLight(), compositeNodeBuilder);
    }

    /// <summary>
    /// Creates a scoped pipeline configuration for multiple NetDaemon light entities.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="lightEntities">The NetDaemon light entities to configure.</param>
    /// <param name="compositeNodeBuilder">An action to configure the pipeline for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator ForLights(this ILightTransitionPipelineConfigurator configurator,
        IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionPipelineConfigurator> compositeNodeBuilder)
    {
        return configurator.ForLights(lightEntities.Select(l => l.AsLight()), compositeNodeBuilder);
    }
}
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for light pipeline configurators to work with NetDaemon light groups.
/// </summary>
public static partial class LightTransitionPipelineConfiguratorExtensions
{
    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within 10 milliseconds.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="lightGroupEntity">The NetDaemon light group entity.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> UseLightGroup(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        ILightEntityCore lightGroupEntity)
    {
        return configurator.UseLightGroup(lightGroupEntity.AsLight());
    }

    /// <summary>
    /// Allows you to provide a group light entity to be used if the same transition is applied to all lights at once within the specified time span.
    /// </summary>
    /// <param name="configurator">The pipeline configurator.</param>
    /// <param name="lightGroupEntity">The NetDaemon light group entity.</param>
    /// <param name="timeSpan">The time span for the transition.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionPipelineConfigurator<NetDaemonLight> UseLightGroup(
        this ILightTransitionPipelineConfigurator<NetDaemonLight> configurator,
        ILightEntityCore lightGroupEntity,
        TimeSpan timeSpan)
    {
        return configurator.UseLightGroup(lightGroupEntity.AsLight(), timeSpan);
    }
}

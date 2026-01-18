using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for light reactive node configurators to work with NetDaemon light entities.
/// </summary>
public static class LightTransitionReactiveNodeConfiguratorExtensions
{
    /// <summary>
    /// Creates a scoped reactive node configuration for a specific NetDaemon light entity.
    /// </summary>
    /// <param name="configurator">The reactive node configurator.</param>
    /// <param name="lightEntity">The NetDaemon light entity to configure.</param>
    /// <param name="configure">An action to configure the reactive node for this specific light.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionReactiveNodeConfigurator<NetDaemonLight> ForLight(
        this ILightTransitionReactiveNodeConfigurator<NetDaemonLight> configurator,
        ILightEntityCore lightEntity, Action<ILightTransitionReactiveNodeConfigurator<NetDaemonLight>> configure)
    {
        return configurator.ForLight(lightEntity.AsLight(), configure);
    }

    /// <summary>
    /// Creates a scoped reactive node configuration for multiple NetDaemon light entities.
    /// </summary>
    /// <param name="configurator">The reactive node configurator.</param>
    /// <param name="lightEntities">The NetDaemon light entities to configure.</param>
    /// <param name="configure">An action to configure the reactive node for these lights.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionReactiveNodeConfigurator<NetDaemonLight> ForLights(
        this ILightTransitionReactiveNodeConfigurator<NetDaemonLight> configurator,
        IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionReactiveNodeConfigurator<NetDaemonLight>> configure)
    {
        return configurator.ForLights(lightEntities.Select(l => l.AsLight()), configure);
    }
}
using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for light cycle configurators to work with NetDaemon light entities.
/// </summary>
public static class LightTransitionCycleConfiguratorExtensions
{
    /// <summary>
    /// Creates a scoped cycle configuration for a specific NetDaemon light entity.
    /// </summary>
    /// <param name="configurator">The cycle configurator.</param>
    /// <param name="lightEntity">The NetDaemon light entity to configure.</param>
    /// <param name="configure">An action to configure the cycle for this specific light.</param>
    /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionCycleConfigurator<NetDaemonLight> ForLight(this ILightTransitionCycleConfigurator<NetDaemonLight> configurator,
        ILightEntityCore lightEntity, Action<ILightTransitionCycleConfigurator<NetDaemonLight>> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        return configurator.ForLight(lightEntity.AsLight(), configure, excludedLightBehaviour);
    }

    /// <summary>
    /// Creates a scoped cycle configuration for multiple NetDaemon light entities.
    /// </summary>
    /// <param name="configurator">The cycle configurator.</param>
    /// <param name="lightEntities">The NetDaemon light entities to configure.</param>
    /// <param name="configure">An action to configure the cycle for these lights.</param>
    /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionCycleConfigurator<NetDaemonLight> ForLights(this ILightTransitionCycleConfigurator<NetDaemonLight> configurator,
        IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionCycleConfigurator<NetDaemonLight>> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        return configurator.ForLights(lightEntities.Select(l => l.AsLight()), configure, excludedLightBehaviour);
    }
}
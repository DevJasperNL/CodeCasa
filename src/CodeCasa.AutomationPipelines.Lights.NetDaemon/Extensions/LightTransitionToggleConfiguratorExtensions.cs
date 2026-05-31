using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Extensions;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for light toggle configurators to work with NetDaemon light entities.
/// </summary>
public static class LightTransitionToggleConfiguratorExtensions
{
    /// <summary>
    /// Creates a scoped toggle configuration for a specific NetDaemon light entity.
    /// </summary>
    /// <param name="configurator">The toggle configurator.</param>
    /// <param name="lightEntity">The NetDaemon light entity to configure.</param>
    /// <param name="configure">An action to configure the toggle for this specific light.</param>
    /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionToggleConfigurator<NetDaemonLight> ForLight(this ILightTransitionToggleConfigurator<NetDaemonLight> configurator,
        ILightEntityCore lightEntity, Action<ILightTransitionToggleConfigurator<NetDaemonLight>> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        return configurator.ForLight(lightEntity.AsLight(), configure, excludedLightBehaviour);
    }

    /// <summary>
    /// Creates a scoped toggle configuration for multiple NetDaemon light entities.
    /// </summary>
    /// <param name="configurator">The toggle configurator.</param>
    /// <param name="lightEntities">The NetDaemon light entities to configure.</param>
    /// <param name="configure">An action to configure the toggle for these lights.</param>
    /// <param name="excludedLightBehaviour">Specifies the behavior for lights not included in this scoped configuration. Defaults to <see cref="ExcludedLightBehaviours.None"/>.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionToggleConfigurator<NetDaemonLight> ForLights(this ILightTransitionToggleConfigurator<NetDaemonLight> configurator,
        IEnumerable<ILightEntityCore> lightEntities, Action<ILightTransitionToggleConfigurator<NetDaemonLight>> configure,
        ExcludedLightBehaviours excludedLightBehaviour = ExcludedLightBehaviours.None)
    {
        return configurator.ForLights(lightEntities.Select(l => l.AsLight()), configure, excludedLightBehaviour);
    }

    /// <summary>
    /// Adds a Home Assistant scene to the toggle sequence. The scene is fetched and cached via
    /// <see cref="LightSceneCacheService"/> on first use. If a light is not part of the scene,
    /// no state is added for that light.
    /// </summary>
    /// <param name="configurator">The toggle configurator.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionToggleConfigurator<NetDaemonLight> AddScene(this ILightTransitionToggleConfigurator<NetDaemonLight> configurator,
        IEntityCore sceneEntity)
    {
        return configurator.Add(sp =>
        {
            var cacheService = sp.GetRequiredService<LightSceneCacheService>();
            var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
            var light = sp.GetRequiredService<ILight>();
            return sceneLights.GetValueOrDefault(light.Id);
        });
    }
}
using CodeCasa.AutomationPipelines.Lights.Cycle;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

public static partial class LightTransitionCycleConfiguratorExtensions
{
    /// <summary>
    /// Adds a Home Assistant scene to the cycle. The scene is fetched and cached via
    /// <see cref="LightSceneCacheService"/> on first use. If a light is not part of the scene,
    /// no state is added for that light in the cycle.
    /// </summary>
    /// <param name="configurator">The cycle configurator.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <param name="comparer">An optional equality comparer for determining if light parameters match the current state. If null, the default equality comparison is used.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ILightTransitionCycleConfigurator<NetDaemonLight> AddScene(this ILightTransitionCycleConfigurator<NetDaemonLight> configurator,
        IEntityCore sceneEntity, IEqualityComparer<LightParameters>? comparer = null)
    {
        comparer ??= EqualityComparer<LightParameters>.Default;
        return configurator.Add(
            sp =>
            {
                var cacheService = sp.GetRequiredService<LightSceneCacheService>();
                var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
                var light = sp.GetRequiredService<ILight>();
                return sceneLights.GetValueOrDefault(light.Id);
            },
            sp =>
            {
                var cacheService = sp.GetRequiredService<LightSceneCacheService>();
                var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
                var light = sp.GetRequiredService<ILight>();
                return sceneLights.TryGetValue(light.Id, out var parameters) && comparer.Equals(light.GetParameters(), parameters);
            });
    }
}

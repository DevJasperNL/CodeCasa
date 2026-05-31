using CodeCasa.AutomationPipelines.Lights.Toggle;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

public static partial class LightTransitionToggleConfiguratorExtensions
{
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

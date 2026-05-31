using CodeCasa.AutomationPipelines.Lights.Switch;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for <see cref="ILightTransitionSwitchFalseConfigurator{TLight}"/> to support Home Assistant scene entities.
/// </summary>
public static class LightTransitionSwitchFalseConfiguratorExtensions
{
    /// <summary>
    /// Specifies a Home Assistant scene to apply when the observable emits <see langword="false"/>.
    /// The scene is fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of the scene, no state is applied.
    /// </summary>
    /// <param name="configurator">The switch false configurator.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    public static void WhenFalse(
        this ILightTransitionSwitchFalseConfigurator<NetDaemonLight> configurator,
        IEntityCore sceneEntity)
    {
        configurator.WhenFalse(sp => GetSceneLightParameters(sp, sceneEntity));
    }

    private static LightParameters? GetSceneLightParameters(IServiceProvider sp, IEntityCore sceneEntity)
    {
        var cacheService = sp.GetRequiredService<LightSceneCacheService>();
        var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
        var light = sp.GetRequiredService<ILight>();
        return sceneLights.GetValueOrDefault(light.Id);
    }
}

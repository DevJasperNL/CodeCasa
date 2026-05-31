using CodeCasa.AutomationPipelines.Lights.Switch;
using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Extension methods for <see cref="ILightTransitionSwitchConfigurator{TLight}"/> to support Home Assistant scene entities.
/// </summary>
public static class LightTransitionSwitchConfiguratorExtensions
{
    /// <summary>
    /// Specifies a Home Assistant scene to apply when the observable emits <see langword="true"/>.
    /// The scene is fetched and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of the scene, no state is applied.
    /// </summary>
    /// <param name="configurator">The switch configurator.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <returns>A configurator for specifying the false branch.</returns>
    public static ILightTransitionSwitchFalseConfigurator<NetDaemonLight> WhenTrue(
        this ILightTransitionSwitchConfigurator<NetDaemonLight> configurator,
        IEntityCore sceneEntity)
    {
        return configurator.WhenTrue(sp => GetSceneLightParameters(sp, sceneEntity));
    }

    private static LightParameters? GetSceneLightParameters(IServiceProvider sp, IEntityCore sceneEntity)
    {
        var cacheService = sp.GetRequiredService<LightSceneCacheService>();
        var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
        var light = sp.GetRequiredService<ILight>();
        return sceneLights.GetValueOrDefault(light.Id);
    }
}

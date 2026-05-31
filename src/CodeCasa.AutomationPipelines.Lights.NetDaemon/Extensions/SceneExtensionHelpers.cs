using CodeCasa.Lights;
using CodeCasa.Lights.NetDaemon.Scenes;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

internal static class SceneExtensionHelpers
{
    internal static LightParameters? GetSceneLightParameters(IServiceProvider sp, IEntityCore sceneEntity)
    {
        var cacheService = sp.GetRequiredService<LightSceneCacheService>();
        var sceneLights = cacheService.GetLightSceneAsync(sceneEntity.EntityId).GetAwaiter().GetResult();
        var light = sp.GetRequiredService<ILight>();
        return sceneLights.GetValueOrDefault(light.Id);
    }
}

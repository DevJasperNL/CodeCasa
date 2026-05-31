using CodeCasa.AutomationPipelines.Lights.Timeline;
using CodeCasa.Lights.NetDaemon.Scenes;
using NetDaemon.HassModel.Entities;
using Occurify;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ITimelineConfigurator"/> that add scene-based light parameter mappings
/// using Home Assistant scenes via NetDaemon.
/// </summary>
public static class TimelineConfiguratorExtensions
{
    /// <summary>
    /// Adds a mapping from a timeline point to light parameters fetched from a Home Assistant scene.
    /// The scene is resolved and cached via <see cref="LightSceneCacheService"/> on first use.
    /// If the current light is not part of the scene, no state is added for that timeline point.
    /// </summary>
    /// <param name="configurator">The timeline configurator.</param>
    /// <param name="timeline">The timeline point.</param>
    /// <param name="sceneEntity">The scene entity whose light parameters will be applied.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    public static ITimelineConfigurator AddScene(this ITimelineConfigurator configurator,
        ITimeline timeline,
        IEntityCore sceneEntity)
    {
        return configurator.Add(timeline, sp => SceneExtensionHelpers.GetSceneLightParameters(sp, sceneEntity));
    }
}

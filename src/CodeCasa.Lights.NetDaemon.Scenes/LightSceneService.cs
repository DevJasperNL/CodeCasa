using CodeCasa.Lights.NetDaemon.Extensions;
using CodeCasa.Lights.NetDaemon.Scenes.Generated;
using NetDaemon.Client;
using NetDaemon.HassModel;

namespace CodeCasa.Lights.NetDaemon.Scenes
{
    /// <summary>
    /// Service for retrieving and converting Home Assistant light scenes to <see cref="LightParameters"/>.
    /// </summary>
    /// <remarks>
    /// This service provides functionality to fetch Home Assistant scene configurations and
    /// convert the light entity states within those scenes to a standardized <see cref="LightParameters"/>
    /// format for use in automation pipelines.
    /// </remarks>
    public class LightSceneService(IHaContext haContext, IHomeAssistantApiManager apiManager)
    {
        /// <summary>
        /// Retrieves the light parameters for all lights in a Home Assistant scene.
        /// </summary>
        /// <param name="sceneEntityId">The entity ID of the Home Assistant scene (e.g., "scene.my_scene").</param>
        /// <param name="cancellationToken">A cancellation token to support cancellation of the asynchronous operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a dictionary mapping
        /// light entity IDs to their corresponding <see cref="LightParameters"/> as defined in the scene.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the scene entity does not have attributes or when the scene configuration cannot be retrieved from Home Assistant.
        /// </exception>
        public async Task<Dictionary<string, LightParameters>> GetLightSceneAsync(string sceneEntityId, CancellationToken cancellationToken = default)
        {
            var sceneEntity = new SceneEntity(haContext, sceneEntityId);
            var internalSceneId = sceneEntity.Attributes?.Id ?? throw new InvalidOperationException($"Scene {sceneEntityId} does not have attributes.");
            var sceneConfig = await GetSceneConfigAsync(internalSceneId, cancellationToken);
            return sceneConfig.Lights.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToLightParameters());
        }

        private async Task<SceneConfig> GetSceneConfigAsync(string sceneId, CancellationToken ct = default)
        {
            return await apiManager.GetApiCallAsync<SceneConfig>($"config/scene/config/{sceneId}", ct) ??
                   throw new InvalidOperationException($"Couldn't retrieve scene {sceneId}.");
        }
    }
}

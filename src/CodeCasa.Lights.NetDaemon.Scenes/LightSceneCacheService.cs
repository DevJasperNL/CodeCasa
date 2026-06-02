using System.Collections.Concurrent;

namespace CodeCasa.Lights.NetDaemon.Scenes
{
    /// <summary>
    /// A transient service that caches light scene data retrieved by <see cref="LightSceneService"/>,
    /// preventing redundant API calls for the same scene entity.
    /// </summary>
    public class LightSceneCacheService(LightSceneService lightSceneService)
    {
        private static readonly ConcurrentDictionary<string, Dictionary<string, LightParameters>> Cache = new();
        private static readonly SemaphoreSlim Lock = new(1, 1);

        /// <summary>
        /// Retrieves the light parameters for all lights in a Home Assistant scene, using a cached result
        /// if the scene has been fetched before.
        /// </summary>
        /// <param name="sceneEntityId">The entity ID of the Home Assistant scene (e.g., "scene.my_scene").</param>
        /// <param name="cancellationToken">A cancellation token to support cancellation of the asynchronous operation.</param>
        /// <returns>
        /// A dictionary mapping light entity IDs to their corresponding <see cref="LightParameters"/> as defined in the scene.
        /// </returns>
        public async Task<Dictionary<string, LightParameters>> GetLightSceneAsync(string sceneEntityId, CancellationToken cancellationToken = default)
        {
            if (Cache.TryGetValue(sceneEntityId, out var cached))
            {
                return cached;
            }

            await Lock.WaitAsync(cancellationToken);
            try
            {
                if (Cache.TryGetValue(sceneEntityId, out cached))
                {
                    return cached;
                }

                var result = await lightSceneService.GetLightSceneAsync(sceneEntityId, cancellationToken);
                Cache[sceneEntityId] = result;
                return result;
            }
            finally
            {
                Lock.Release();
            }
        }
    }
}

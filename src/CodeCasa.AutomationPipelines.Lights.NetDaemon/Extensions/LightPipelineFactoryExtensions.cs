using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights.NetDaemon;
using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="LightPipelineFactory"/> to work with NetDaemon light entities.
    /// </summary>
    public static class LightPipelineFactoryExtensions
    {
        /// <summary>
        /// Sets up a light pipeline for a NetDaemon light entity.
        /// </summary>
        /// <param name="lightPipelineFactory">The light pipeline factory.</param>
        /// <param name="lightEntity">The NetDaemon light entity to set up the pipeline for.</param>
        /// <param name="pipelineBuilder">An action to configure the pipeline behavior.</param>
        /// <returns>An async disposable representing the created pipeline(s) that can be disposed to clean up resources.</returns>
        public static IAsyncDisposable SetupLightPipeline(this LightPipelineFactory lightPipelineFactory, ILightEntityCore lightEntity,
            Action<ILightTransitionPipelineConfigurator<NetDaemonLight>> pipelineBuilder)
        {
            return lightPipelineFactory.SetupLightPipeline(lightEntity.AsLight(), pipelineBuilder);
        }
    }
}

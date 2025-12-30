using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.AutomationPipelines.Lights.NetDaemon.Extensions
{
    public static class LightPipelineFactoryExtensions
    {
        public static IAsyncDisposable SetupLightPipeline(this LightPipelineFactory lightPipelineFactory, ILightEntityCore lightEntity,
            Action<ILightTransitionPipelineConfigurator> pipelineBuilder)
        {
            return lightPipelineFactory.SetupLightPipeline(lightEntity.AsLight(), pipelineBuilder);
        }
    }
}

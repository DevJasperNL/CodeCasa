using CodeCasa.AutomationPipelines.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    public ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer)
    {
        NodeContainers.Values.ForEach(b => b.AddTelemetrySubscriber(observer));
        return this;
    }
}
using CodeCasa.AutomationPipelines.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight>
{
    public ILightTransitionPipelineConfigurator<TLight> AddTelemetryObserver(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer)
    {
        NodeContainers.Values.ForEach(b => b.AddTelemetryObserver(observer));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetryObserver(Action<LightTransitionPipelineTelemetry<TLight>> onNext)
    {
        NodeContainers.Values.ForEach(b => b.AddTelemetryObserver(onNext));
        return this;
    }
}
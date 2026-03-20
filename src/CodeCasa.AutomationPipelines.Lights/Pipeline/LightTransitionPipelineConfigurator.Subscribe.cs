namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    internal List<IObserver<LightTransitionPipelineTelemetry<TLight>>> Observers { get; } = new();

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer)
    {
        Observers.Add(observer);
        return this;
    }
}
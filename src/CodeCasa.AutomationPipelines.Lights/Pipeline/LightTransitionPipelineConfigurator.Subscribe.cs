using System.Reactive;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    internal List<IObserver<LightTransitionPipelineTelemetry<TLight>>> Observers { get; } = new();

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetryObserver(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer)
    {
        Observers.Add(observer);
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetryObserver(
        Action<LightTransitionPipelineTelemetry<TLight>> onNext)
    {
        Observers.Add(Observer.Create(onNext));
        return this;
    }
}
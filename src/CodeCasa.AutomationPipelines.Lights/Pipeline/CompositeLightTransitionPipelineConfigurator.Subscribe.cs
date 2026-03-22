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

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        Action<LightTransitionPipelineTelemetry<TLight>> onNext)
    {
        NodeContainers.Values.ForEach(b => b.AddTelemetrySubscriber(onNext));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> ConfigureTelemetrySubscriber(Func<IObservable<LightTransitionPipelineTelemetry<TLight>>, IDisposable> subscriptionFactory)
    {
        NodeContainers.Values.ForEach(b => b.ConfigureTelemetrySubscriber(subscriptionFactory));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> OnCompleted(Action<LightTransitionPipelineCreatedEvent<TLight>> callback)
    {
        NodeContainers.Values.ForEach(b => b.OnCompleted(callback));
        return this;
    }
}
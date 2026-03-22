namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight>
{
    internal List<Func<IObservable<LightTransitionPipelineTelemetry<TLight>>, IDisposable>> TelemetrySubscriptionFactories { get; } = new();
    internal List<Action<LightTransitionPipelineCreatedEvent<TLight>>> PipelineCompletedCallbacks { get; } = new();

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer)
    {
        TelemetrySubscriptionFactories.Add(observable => observable.Subscribe(observer));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        Action<LightTransitionPipelineTelemetry<TLight>> onNext)
    {
        TelemetrySubscriptionFactories.Add(observable => observable.Subscribe(onNext));
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> ConfigureTelemetrySubscriber(Func<IObservable<LightTransitionPipelineTelemetry<TLight>>, IDisposable> subscriptionFactory)
    {
        TelemetrySubscriptionFactories.Add(subscriptionFactory);
        return this;
    }

    public ILightTransitionPipelineConfigurator<TLight> OnCompleted(Action<LightTransitionPipelineCreatedEvent<TLight>> callback)
    {
        PipelineCompletedCallbacks.Add(callback);
        return this;
    }
}
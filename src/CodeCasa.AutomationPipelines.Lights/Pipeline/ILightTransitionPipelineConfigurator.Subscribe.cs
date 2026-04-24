namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight>
{
    /// <summary>
    /// Adds a telemetry subscriber to observe state transitions within the light transition pipeline.
    /// The observer will receive notifications for every state transition between pipeline nodes.
    /// </summary>
    /// <param name="observer">The observer that will receive telemetry data about state transitions.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    /// <remarks>
    /// The lifetime of the subscription is tied to the lifetime of the pipeline.
    /// When the pipeline is disposed, the subscription will automatically be terminated.
    /// </remarks>
    ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer);

    /// <summary>
    /// Adds a telemetry subscriber using an action callback to observe state transitions within the light transition pipeline.
    /// The callback will be invoked for every state transition between pipeline nodes.
    /// </summary>
    /// <param name="onNext">The callback action that will be invoked with telemetry data for each state transition.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    /// <remarks>
    /// The lifetime of the subscription is tied to the lifetime of the pipeline.
    /// When the pipeline is disposed, the subscription will automatically be terminated.
    /// </remarks>
    ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        Action<LightTransitionPipelineTelemetry<TLight>> onNext);

    /// <summary>
    /// Adds a telemetry subscriber using a custom subscription factory to observe state transitions within the light transition pipeline.
    /// This overload provides full control over subscription creation and management.
    /// </summary>
    /// <param name="subscriptionFactory">A factory function that receives the telemetry observable and returns a subscription.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    /// <remarks>
    /// The lifetime of the subscription is tied to the lifetime of the pipeline.
    /// When the pipeline is disposed, the subscription will automatically be terminated.
    /// Use this overload when you need custom observable operators (e.g., filtering, throttling, buffering) before subscribing.
    /// </remarks>
    ILightTransitionPipelineConfigurator<TLight> ConfigureTelemetrySubscriber(
        Func<IObservable<LightTransitionPipelineTelemetry<TLight>>, IDisposable> subscriptionFactory);

    /// <summary>
    /// Registers a callback to be invoked when the pipeline has been fully created.
    /// </summary>
    /// <param name="callback">The callback action invoked with the pipeline created event when the pipeline construction is complete.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> OnCompleted(
        Action<LightTransitionPipelineCreatedEvent<TLight>> callback);

}
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
    /// The lifetime of the subscription is connected to the lifetime of the pipeline.
    /// When the pipeline is disposed, the subscription will automatically be terminated.
    /// </remarks>
    ILightTransitionPipelineConfigurator<TLight> AddTelemetrySubscriber(
        IObserver<LightTransitionPipelineTelemetry<TLight>> observer);
}
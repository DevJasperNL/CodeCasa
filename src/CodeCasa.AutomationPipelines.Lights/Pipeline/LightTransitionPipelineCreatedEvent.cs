using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

/// <summary>
/// Represents the event data emitted when a light transition pipeline has been fully created.
/// </summary>
/// <typeparam name="TLight">The specific type of light controlled by the pipeline.</typeparam>
/// <param name="Pipeline">The fully created pipeline instance.</param>
/// <param name="Light">The light controlled by the pipeline.</param>
public record LightTransitionPipelineCreatedEvent<TLight>(IPipeline<LightTransition> Pipeline, TLight Light) where TLight : ILight;
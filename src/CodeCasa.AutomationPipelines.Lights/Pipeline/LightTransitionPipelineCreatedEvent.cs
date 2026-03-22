using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public record LightTransitionPipelineCreatedEvent<TLight>(IPipeline<LightTransition> Pipeline, TLight Light) where TLight : ILight;
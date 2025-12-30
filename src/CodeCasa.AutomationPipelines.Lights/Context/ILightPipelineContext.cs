using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

/// <summary>
/// Represents the context for a light pipeline, providing access to the service provider and the light entity being controlled.
/// </summary>
public interface ILightPipelineContext
{
    /// <summary>
    /// Gets the service provider instance used to resolve dependencies in the pipeline.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the light entity being controlled by the pipeline.
    /// </summary>
    ILight LightEntity { get; }
}
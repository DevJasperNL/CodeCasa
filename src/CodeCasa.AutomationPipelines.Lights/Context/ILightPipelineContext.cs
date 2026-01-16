using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

/// <summary>
/// Represents the context for a light pipeline, providing access to the service provider and the light being controlled.
/// </summary>
/// <typeparam name="TLight">The specific type of light being controlled, which must implement <see cref="ILight"/>.</typeparam>
public interface ILightPipelineContext<out TLight> where TLight : ILight
{
    /// <summary>
    /// Gets the service provider instance used to resolve dependencies in the pipeline.
    /// </summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the light being controlled by the pipeline.
    /// </summary>
    TLight Light { get; }
}
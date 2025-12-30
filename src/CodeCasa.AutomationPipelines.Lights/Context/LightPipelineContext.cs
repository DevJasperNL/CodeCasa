using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

/// <inheritdoc />
public class LightPipelineContext : ILightPipelineContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LightPipelineContext"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider instance used to resolve dependencies in the pipeline.</param>
    /// <param name="lightEntity">The light entity being controlled by the pipeline.</param>
    internal LightPipelineContext(IServiceProvider serviceProvider, ILight lightEntity)
    {
        ServiceProvider = serviceProvider;
        LightEntity = lightEntity;
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public ILight LightEntity { get; }
}
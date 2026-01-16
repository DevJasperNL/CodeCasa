using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

/// <inheritdoc />
public class LightPipelineContext<TLight>(IServiceProvider serviceProvider, TLight light) : ILightPipelineContext<TLight>
    where TLight : ILight
{
    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <inheritdoc />
    public TLight Light { get; } = light;
}
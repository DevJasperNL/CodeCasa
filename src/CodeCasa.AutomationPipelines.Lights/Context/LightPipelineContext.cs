using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Context;

///// <inheritdoc />
//public class LightPipelineContext(IServiceProvider serviceProvider, ILight light) : ILightPipelineContext
//{
//    /// <inheritdoc />
//    public IServiceProvider ServiceProvider { get; } = serviceProvider;

//    /// <inheritdoc />
//    public ILight Light { get; } = light;
//}

/// <inheritdoc />
public class LightPipelineContext<TLight>(IServiceProvider serviceProvider) : ILightPipelineContext<TLight>
    where TLight : ILight
{
    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <inheritdoc />
    //public TLight Light { get; } = light;
}
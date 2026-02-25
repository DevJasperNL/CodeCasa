namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

public partial interface ILightTransitionPipelineConfigurator<TLight>
{
    /// <summary>
    /// Enables logging for the pipeline configuration.
    /// </summary>
    /// <param name="pipelineName">The optional name of the pipeline to include in logs.</param>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> EnableLogging(string? pipelineName = null);

    /// <summary>
    /// Disables logging for the pipeline configuration.
    /// </summary>
    /// <returns>The configurator instance for method chaining.</returns>
    ILightTransitionPipelineConfigurator<TLight> DisableLogging();
}
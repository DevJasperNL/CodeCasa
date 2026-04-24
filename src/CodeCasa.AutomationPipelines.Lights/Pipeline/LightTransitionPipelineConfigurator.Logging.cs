namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight> : IPipelineHierarchyContext
{
    private string? _parentName;

    public string HierarchyPath => _parentName == null ? Name ?? "Pipeline" : $"{_parentName}->{Name ?? "Pipeline"}";
    public bool? LoggingEnabled { get; private set; }

    public void EnableLoggingInternal()
    {
        LoggingEnabled = true;
    }

    public void SetParentName(string parentName)
    {
        _parentName = parentName;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> EnableLogging(string? pipelineName = null)
    {
        Name = pipelineName;
        LoggingEnabled = true;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> DisableLogging()
    {
        LoggingEnabled = false;
        return this;
    }
}

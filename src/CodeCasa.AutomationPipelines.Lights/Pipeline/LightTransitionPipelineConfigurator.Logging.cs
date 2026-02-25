namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class LightTransitionPipelineConfigurator<TLight> : IInternalLoggingContext
{
    private string? _parentName;
    private string? _name;

    public string LogName => _parentName == null ? _name ?? "Pipeline" : $"{_parentName}->{_name ?? "Pipeline"}";
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
        _name = name;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> EnableLogging(string? pipelineName = null)
    {
        _name = pipelineName;
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

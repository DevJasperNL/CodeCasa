using CodeCasa.AutomationPipelines.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal partial class CompositeLightTransitionPipelineConfigurator<TLight> : IInternalLoggingContext
{
    private string? _parentName;
    private string? _name;

    public string LogName => _parentName == null ? _name ?? "Pipeline" : $"{_parentName}->{_name ?? "Pipeline"}";
    public bool? LoggingEnabled { get; private set; }

    public void EnableLoggingInternal()
    {
        LoggingEnabled = true;
        NodeContainers.Values.ForEach(b => b.EnableLoggingInternal());
    }

    public void SetParentName(string parentName)
    {
        _parentName = parentName;
        NodeContainers.Values.ForEach(b => b.SetParentName(parentName));
    }

    public void SetName(string name)
    {
        _name = name;
        NodeContainers.Values.ForEach(b => b.SetName(name));
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> EnableLogging(string? pipelineName = null)
    {
        _name = pipelineName;
        LoggingEnabled = true;
        NodeContainers.Values.ForEach(b => b.EnableLogging(pipelineName));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionPipelineConfigurator<TLight> DisableLogging()
    {
        LoggingEnabled = false;
        NodeContainers.Values.ForEach(b => b.DisableLogging());
        return this;
    }
}

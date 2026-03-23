namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator<TLight> : IPipelineHierarchyContext
{
    private string? _parentName;

    public string HierarchyPath => _parentName == null ? Name ?? "Node" : $"{_parentName}->{Name ?? "Node"}";
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
    public ILightTransitionReactiveNodeConfigurator<TLight> EnableLogging(string? name = null)
    {
        Name = name;
        LoggingEnabled = true;
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> DisableLogging()
    {
        LoggingEnabled = false;
        return this;
    }
}

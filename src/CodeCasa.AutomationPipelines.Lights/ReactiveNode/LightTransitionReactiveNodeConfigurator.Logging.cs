namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class LightTransitionReactiveNodeConfigurator<TLight> : IInternalLoggingContext
{
    private string? _parentName;
    private string? _name;

    public string LogName => _parentName == null ? _name ?? "Node" : $"{_parentName}->{_name ?? "Node"}";
    public bool? LoggingEnabled { get; private set; }

    public void EnableLoggingInternal()
    {
        LoggingEnabled = true;
    }

    public void SetParentName(string parentName)
    {
        _parentName = parentName;
    }

    public void SetNameInternal(string name)
    {
        _name = name;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> EnableLogging(string? name = null)
    {
        _name = name;
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

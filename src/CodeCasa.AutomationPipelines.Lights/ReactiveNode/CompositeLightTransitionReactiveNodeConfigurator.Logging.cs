using CodeCasa.AutomationPipelines.Lights.Extensions;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

internal partial class CompositeLightTransitionReactiveNodeConfigurator<TLight> : IInternalLoggingContext
{
    private string? _parentName;
    private string? _name;

    public string LogName => _parentName == null ? _name ?? "Node" : $"{_parentName}->{_name ?? "Node"}";
    public bool? LoggingEnabled { get; private set; }

    public void EnableLoggingInternal()
    {
        LoggingEnabled = true;
        configurators.Values.ForEach(b => b.EnableLoggingInternal());
    }

    public void SetParentName(string parentName)
    {
        _parentName = parentName;
        configurators.Values.ForEach(b => b.SetParentName(parentName));
    }

    public void SetName(string name)
    {
        _name = name;
        configurators.Values.ForEach(b => b.SetName(name));
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> EnableLogging(string? name = null)
    {
        _name = name;
        LoggingEnabled = true;
        configurators.Values.ForEach(b => b.EnableLogging(name));
        return this;
    }

    /// <inheritdoc/>
    public ILightTransitionReactiveNodeConfigurator<TLight> DisableLogging()
    {
        LoggingEnabled = false;
        configurators.Values.ForEach(b => b.DisableLogging());
        return this;
    }
}

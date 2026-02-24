
namespace CodeCasa.AutomationPipelines.Lights;

internal interface IInternalLoggingContext
{
    void EnableLoggingInternal();
    void SetNameInternal(string name);
    void SetParentName(string parentName);
    string LogName { get; }
    bool? LoggingEnabled { get; }
}
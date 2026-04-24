
namespace CodeCasa.AutomationPipelines.Lights;

internal interface IPipelineHierarchyContext
{
    void EnableLoggingInternal();
    void SetName(string name);
    void SetParentName(string parentName);
    string HierarchyPath { get; }
    bool? LoggingEnabled { get; }
}
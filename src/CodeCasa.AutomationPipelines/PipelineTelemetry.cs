
namespace CodeCasa.AutomationPipelines
{
    public record PipelineTelemetry<TState>(
        int? SourceNodeIndex,
        string? SourceNodeName,
        int? DestinationNodeIndex,
        string? DestinationNodeName,
        TState? StateValue
    );
}

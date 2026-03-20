
namespace CodeCasa.AutomationPipelines
{
    /// <summary>
    /// Represents telemetry data for a state transition within a pipeline.
    /// </summary>
    /// <typeparam name="TState">The type of state flowing through the pipeline.</typeparam>
    /// <param name="SourceNodeIndex">The index of the source node, or null if the state originates from pipeline input.</param>
    /// <param name="SourceNodeName">The name of the source node, or null if the state originates from pipeline input.</param>
    /// <param name="DestinationNodeIndex">The index of the destination node, or null if the state is being passed to pipeline output.</param>
    /// <param name="DestinationNodeName">The name of the destination node, or null if the state is being passed to pipeline output.</param>
    /// <param name="StateValue">The state value being transitioned.</param>
    public record PipelineTelemetry<TState>(
        int? SourceNodeIndex,
        string? SourceNodeName,
        int? DestinationNodeIndex,
        string? DestinationNodeName,
        TState? StateValue
    );
}

using System.Collections.Immutable;

namespace CodeCasa.AutomationPipelines;

/// <summary>
/// Represents telemetry data for a state transition within a pipeline.
/// </summary>
/// <typeparam name="TState">The type of state flowing through the pipeline.</typeparam>
/// <param name="SourceNodeIndex">The index of the source node, or null if the state originates from pipeline input.</param>
/// <param name="SourceNodeName">The name of the source node, or null if the state originates from pipeline input.</param>
/// <param name="DestinationNodeIndex">The index of the destination node, or null if the state is being passed to pipeline output.</param>
/// <param name="DestinationNodeName">The name of the destination node, or null if the state is being passed to pipeline output.</param>
/// <param name="StateValue">The state value being transitioned.</param>
/// <param name="NestingPath">The path of node indices from root pipeline to this telemetry event. Empty for root-level events.</param>
/// <param name="NestingNames">The names of pipelines in the nesting path.</param>
public record PipelineTelemetry<TState>(
    int? SourceNodeIndex,
    string? SourceNodeName,
    int? DestinationNodeIndex,
    string? DestinationNodeName,
    TState? StateValue,
    ImmutableArray<int> NestingPath = default,
    ImmutableArray<string> NestingNames = default
)
{
    /// <summary>
    /// Gets the nesting depth. 0 = root pipeline.
    /// </summary>
    public int Depth => NestingPath.IsDefaultOrEmpty ? 0 : NestingPath.Length;

    /// <summary>
    /// Creates a new telemetry event with this event nested under the specified parent pipeline.
    /// </summary>
    public PipelineTelemetry<TState> WithParentPipeline(int parentNodeIndex, string parentPipelineName) =>
        this with
        {
            NestingPath = NestingPath.IsDefaultOrEmpty
                ? [parentNodeIndex]
                : NestingPath.Insert(0, parentNodeIndex),
            NestingNames = NestingNames.IsDefaultOrEmpty
                ? [parentPipelineName]
                : NestingNames.Insert(0, parentPipelineName)
        };
}

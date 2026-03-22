
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    /// <summary>
    /// Represents telemetry data for a state transition within a pipeline.
    /// </summary>
    /// <typeparam name="TLight">The light controlled by the pipeline.</typeparam>
    /// <param name="SourceNodeIndex">The index of the source node, or null if the state originates from pipeline input.</param>
    /// <param name="SourceNodeName">The name of the source node, or null if the state originates from pipeline input.</param>
    /// <param name="DestinationNodeIndex">The index of the destination node, or null if the state is being passed to pipeline output.</param>
    /// <param name="DestinationNodeName">The name of the destination node, or null if the state is being passed to pipeline output.</param>
    /// <param name="StateValue">The state value being transitioned.</param>
    public record LightTransitionPipelineTelemetry<TLight>(
        IPipeline<LightTransition> Pipeline,
        TLight Light,
        int? SourceNodeIndex,
        string? SourceNodeName,
        int? DestinationNodeIndex,
        string? DestinationNodeName,
        LightTransition? StateValue
    ) where TLight : ILight;
}

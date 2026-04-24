using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline
{
    internal class PipelineLogger<TState>(ILogger<Pipeline<TState>>? logger, string? name)
    {
        public void Log(PipelineTelemetry<TState> pipelineTelemetry)
        {
            if (pipelineTelemetry.Depth != 0)
            {
                // We only log the root pipeline telemetry to allow users to configure logging for nested pipelines separately if they choose to.
                return;
            }
            if (pipelineTelemetry.SourceNodeIndex == null && pipelineTelemetry.DestinationNodeIndex == null)
            {
                logger?.LogTrace($"{LogPrefix}Input set to [{pipelineTelemetry.StateValue?.ToString() ?? "NULL"}]. No nodes registered, passing to pipeline output immediately.");
                return;
            }

            if (pipelineTelemetry.SourceNodeIndex == null)
            {
                logger?.LogTrace($"{LogPrefix}Input set to [{pipelineTelemetry.StateValue?.ToString() ?? "NULL"}]. Passing input to first [Node {pipelineTelemetry.DestinationNodeIndex}] ({pipelineTelemetry.DestinationNodeName}).");
                return;
            }
            if (pipelineTelemetry.DestinationNodeIndex == null)
            {
                logger?.LogTrace(
                    $"{LogPrefix}[Node {pipelineTelemetry.SourceNodeIndex}] ({pipelineTelemetry.SourceNodeName}) passed value [{pipelineTelemetry.StateValue?.ToString() ?? "NULL"}] to pipeline output.");
                return;
            }
            logger?.LogTrace($"{LogPrefix}Passing [Node {pipelineTelemetry.SourceNodeIndex}] ({pipelineTelemetry.SourceNodeName}) value [{pipelineTelemetry.StateValue?.ToString() ?? "NULL"}] to [Node {pipelineTelemetry.DestinationNodeIndex}] ({pipelineTelemetry.DestinationNodeName}).");
        }

        private string LogPrefix => name == null ? "" : $"{name}: ";
    }
}

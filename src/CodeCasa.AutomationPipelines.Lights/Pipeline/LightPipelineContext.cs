using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

/// <summary>
/// Represents a consistent snapshot of a light pipeline's last output.
/// </summary>
public record LightPipelineState(LightTransition? Output, DateTimeOffset OutputSetAt);

/// <summary>
/// Provides context about the current state of a light pipeline.
/// Can be injected into pipeline nodes to retrieve the last output state and when it was set.
/// </summary>
public class LightPipelineContext
{
    private volatile LightPipelineState? _state;

    /// <summary>
    /// Gets a consistent snapshot of the last pipeline output and the time it was set,
    /// or <c>null</c> if no output has been produced yet.
    /// </summary>
    public LightPipelineState? State => _state;

    internal void Update(LightTransition? output, DateTimeOffset timestamp)
    {
        _state = new LightPipelineState(output, timestamp);
    }
}

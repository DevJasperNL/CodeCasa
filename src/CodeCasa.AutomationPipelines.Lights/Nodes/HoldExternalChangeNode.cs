using System.Reactive.Concurrency;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

/// <summary>
/// Outputs the state a light was put in externally, until the next input from upstream or until an optional timeout.
/// </summary>
internal sealed class HoldExternalChangeNode : PipelineNode<LightTransition>
{
    private readonly IDisposable? _timeout;

    public HoldExternalChangeNode(LightParameters heldParameters, TimeSpan? holdTimeout, IScheduler scheduler)
    {
        Name = "Hold External Change Node";
        Output = heldParameters.AsTransition();
        if (holdTimeout != null)
        {
            _timeout = scheduler.Schedule(holdTimeout.Value, () => PassThrough = true);
        }
    }

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input)
    {
        // The first input is the one pushed when this node is activated; the one after that is an upstream change.
        TurnOnPassThroughOnNextInput();
    }

    /// <inheritdoc />
    public override ValueTask DisposeAsync()
    {
        _timeout?.Dispose();
        return base.DisposeAsync();
    }
}

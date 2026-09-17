using System.Runtime.CompilerServices;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class GroupNode : PipelineNode<LightTransition>
{
    private static readonly object AppliedByGroupMarker = new();
    private readonly ILight _light;
    private readonly GroupNodeContext _groupNodeContext;
    private readonly IEqualityComparer<LightTransition>? _distinctEqualityComparer;
    private readonly ConditionalWeakTable<LightTransition, object> _appliedByGroup = new();

    public GroupNode(ILight light, GroupNodeContext groupNodeContext, IEqualityComparer<LightTransition>? distinctEqualityComparer = null)
    {
        _light = light;
        _groupNodeContext = groupNodeContext;
        _distinctEqualityComparer = distinctEqualityComparer;
        Name = "Group Node";
    }

    /// <summary>
    /// True when the pipeline uses distinct output and would not send <paramref name="transition"/> to the light because it
    /// equals the current output.
    /// </summary>
    internal bool IsSuppressedAsDuplicate(LightTransition transition) =>
        _distinctEqualityComparer != null && _distinctEqualityComparer.Equals(Output, transition);

    /// <summary>
    /// Returns true when <paramref name="transition"/> is an output this node emitted because the group entity already
    /// received it. The pipeline output handler uses this to keep <c>Output</c>, telemetry and context up to date without
    /// also sending the transition to the individual light.
    /// </summary>
    /// <remarks>
    /// The mark travels with the emitted instance rather than being a flag that is only set during emission: the pipeline
    /// may process the output later on another thread, after the flag would already have been reset.
    /// </remarks>
    internal bool WasAppliedByGroup(LightTransition transition) => _appliedByGroup.TryGetValue(transition, out _);

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input) => _groupNodeContext.Process(this, input);

    internal void SetOutput(LightTransition? output, bool appliedByGroup = false)
    {
        if (appliedByGroup && output != null)
        {
            // A fresh instance, so an equal transition emitted individually later is never mistaken for this one.
            output = output with { };
            _appliedByGroup.AddOrUpdate(output, AppliedByGroupMarker);
        }
        Output = output;
    }

    /// <summary>
    /// Sends <paramref name="transition"/> to the light without going through the pipeline output handler. Used when the
    /// output was already emitted as applied by the group but the group call failed: emitting it again would be suppressed
    /// as a duplicate by a distinct pipeline.
    /// </summary>
    internal void ApplyIndividually(LightTransition transition)
    {
        // The queued group action may run after this node's pipeline was disposed; SetOutput is already a no-op then.
        if (IsDisposed)
        {
            return;
        }
        _light.ApplyTransition(transition);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _groupNodeContext.Unregister(this);
        await base.DisposeAsync();
    }
}

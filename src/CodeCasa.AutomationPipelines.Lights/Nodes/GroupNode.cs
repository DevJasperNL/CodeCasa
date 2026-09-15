using System.Runtime.CompilerServices;
using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class GroupNode : PipelineNode<LightTransition>
{
    private static readonly object AppliedByGroupMarker = new();
    private readonly GroupNodeContext _groupNodeContext;
    private readonly IEqualityComparer<LightTransition>? _distinctEqualityComparer;
    private readonly ConditionalWeakTable<LightTransition, object> _appliedByGroup = new();

    public GroupNode(GroupNodeContext groupNodeContext, IEqualityComparer<LightTransition>? distinctEqualityComparer = null)
    {
        _groupNodeContext = groupNodeContext;
        _distinctEqualityComparer = distinctEqualityComparer;
        Name = "Group Node";
    }

    /// <summary>
    /// True when the pipeline's startup behaviour leaves the light alone, so its startup outputs must not reach the group entity.
    /// </summary>
    internal bool SkipsInitialOutput { get; init; }

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
    protected override void InputReceived(LightTransition? input)
    {
        if (input != null)
        {
            _groupNodeContext.Process(this, input);
        }
    }

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

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _groupNodeContext.Unregister(this);
        await base.DisposeAsync();
    }
}

using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class GroupNode : PipelineNode<LightTransition>
{
    private readonly GroupNodeContext _groupNodeContext;
    private readonly IEqualityComparer<LightTransition>? _distinctEqualityComparer;

    public GroupNode(GroupNodeContext groupNodeContext, IEqualityComparer<LightTransition>? distinctEqualityComparer = null)
    {
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
    /// True while the current output is being emitted because the group entity already received the transition.
    /// The pipeline output handler uses this to keep <c>Output</c>, telemetry and context up to date without also
    /// sending the transition to the individual light.
    /// </summary>
    internal bool OutputAppliedByGroup { get; private set; }

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
        OutputAppliedByGroup = appliedByGroup;
        try
        {
            Output = output;
        }
        finally
        {
            OutputAppliedByGroup = false;
        }
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _groupNodeContext.Unregister(this);
        await base.DisposeAsync();
    }
}

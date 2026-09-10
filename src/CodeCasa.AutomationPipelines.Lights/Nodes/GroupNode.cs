using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class GroupNode : PipelineNode<LightTransition>
{
    private readonly GroupNodeContext _groupNodeContext;

    public GroupNode(GroupNodeContext groupNodeContext)
    {
        _groupNodeContext = groupNodeContext;
        Name = "Group Node";
    }

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

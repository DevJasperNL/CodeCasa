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

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input)
    {
        if (input != null)
        {
            _groupNodeContext.Process(this, input);
        }
    }

    internal void SetOutput(LightTransition? output)
    {
        Output = output;
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _groupNodeContext.Unregister(this);
        await base.DisposeAsync();
    }
}
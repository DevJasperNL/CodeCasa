using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class GroupNode : PipelineNode<LightTransition>
{
    private readonly GroupNodeContext _groupNodeContext;
    private readonly ILight _light;

    public GroupNode(ILight light, GroupNodeContext groupNodeContext)
    {
        _light = light;
        _groupNodeContext = groupNodeContext;
        Name = "Group Node";
    }

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input)
    {
        if (input != null)
        {
            _groupNodeContext.Process(_light, input);
        }
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _groupNodeContext.Unregister(_light);
        await base.DisposeAsync();
    }
}
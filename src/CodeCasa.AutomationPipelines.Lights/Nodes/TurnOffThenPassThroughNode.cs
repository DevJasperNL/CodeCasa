using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class TurnOffThenPassThroughNode : PipelineNode<LightTransition>
{
    public TurnOffThenPassThroughNode()
    {
        // Note: we cannot simply call ChangeOutputAndTurnOnPassThroughOnNextInput here, as the input will immediately be set when this node is added to the timeline.
        Output = LightTransition.Off();
    }

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input)
    {
        TurnOnPassThroughOnNextInput();
    }
}
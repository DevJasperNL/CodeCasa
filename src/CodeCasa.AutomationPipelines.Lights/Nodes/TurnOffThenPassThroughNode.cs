using CodeCasa.Lights;

namespace CodeCasa.AutomationPipelines.Lights.Nodes;

/// <summary>
/// A pipeline node that initially outputs <see cref="LightTransition.Off()"/>, 
/// then switches to pass-through mode after receiving its first input.
/// </summary>
/// <remarks>
/// This node is useful for scenarios where a light should be turned off,
/// but then forward subsequent inputs from upstream nodes without modification.
/// The pass-through behavior is activated upon receiving the first input.
/// </remarks>
public sealed class TurnOffThenPassThroughNode : PipelineNode<LightTransition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TurnOffThenPassThroughNode"/> class.
    /// </summary>
    /// <remarks>
    /// The initial output is set to <see cref="LightTransition.Off()"/>.
    /// Pass-through mode is not enabled in the constructor because the input 
    /// is immediately set when this node is added to the timeline.
    /// </remarks>
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
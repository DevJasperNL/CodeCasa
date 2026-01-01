namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class FactoryNode<TState>(Func<TState?, TState?> stateFactory)
    : PipelineNode<TState>
{
    /// <inheritdoc />
    protected override void InputReceived(TState? input)
    {
        Output = stateFactory(input);
    }
}
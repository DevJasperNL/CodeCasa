namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class FactoryNode<TState> : PipelineNode<TState>
{
    private readonly Func<TState?, TState?> _stateFactory;

    public FactoryNode(Func<TState?, TState?> stateFactory)
    {
        _stateFactory = stateFactory;
        Name = "Factory Node";
    }

    /// <inheritdoc />
    protected override void InputReceived(TState? input)
    {
        Output = _stateFactory(input);
    }
}
namespace CodeCasa.AutomationPipelines.Lights.Nodes;

internal class FactoryNode<TState>(Func<TState?, TState?> lightTransitionFactory)
    : PipelineNode<TState>
{
    /// <inheritdoc />
    protected override void InputReceived(TState? input)
    {
        Output = lightTransitionFactory(input);
    }
}
namespace CodeCasa.Pipeline;

public interface IPipeline<TState> : IPipelineNode<TState>
{
    IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>;
    IPipeline<TState> SetOutputHandler(Action<TState> action);
}
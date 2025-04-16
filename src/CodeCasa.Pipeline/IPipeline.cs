namespace CodeCasa.Pipeline;

public interface IPipeline<TState> : IPipelineNode<TState>
{
    /// <summary>
    /// Sets the default state for the pipeline. This state will be used as the initial input for the first node in the pipeline.
    /// </summary>
    IPipeline<TState> SetDefault(TState state);

    /// <summary>
    /// Registers a new node in the pipeline. The node will be created using the service provider.
    /// </summary>
    IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>;

    /// <summary>
    /// Registers a new node in the pipeline. The node is passed as a parameter.
    /// </summary>
    IPipeline<TState> RegisterNode<TNode>(TNode node) where TNode : IPipelineNode<TState>;

    /// <summary>
    /// Sets the output handler for the pipeline. This handler will be called whenever a new output is produced by the pipeline.
    /// This method can be called at any time during the creation of the pipeline and will be called immediately if the pipeline has already produced an output.
    /// </summary>
    IPipeline<TState> SetOutputHandler(Action<TState> action, bool callActionDistinct = true);
}
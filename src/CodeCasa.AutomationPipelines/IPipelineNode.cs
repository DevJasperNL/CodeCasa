
namespace CodeCasa.AutomationPipelines;

/// <summary>
/// Represents a node in a pipeline.
/// </summary>
public interface IPipelineNode<TState> : IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier of the node.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets or sets the optional name of the node, used for identification and logging purposes.
    /// </summary>
    string? Name { get; set; }
    /// <summary>
    /// Gets or sets the input state of the node. This will trigger the processing of the input.
    /// </summary>
    TState? Input { get; set; }

    /// <summary>
    /// Gets the output state of the node.
    /// </summary>
    TState? Output { get; }

    /// <summary>
    /// Notifies when a new output is produced by the node.
    /// </summary>
    IObservable<TState?> OnNewOutput { get; }
}
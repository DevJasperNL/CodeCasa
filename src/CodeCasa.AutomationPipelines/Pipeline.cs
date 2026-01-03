using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines;

/// <summary>
/// Represents a pipeline of nodes.
/// </summary>
public class Pipeline<TState> : PipelineNode<TState>, IPipeline<TState>
{
    private readonly Lock _lock = new();
    private readonly List<IPipelineNode<TState>> _nodes = new();
    private readonly ILogger<Pipeline<TState>>? _logger;

    private bool _callActionDistinct = true;
    private Action<TState>? _action;
    private IDisposable? _subscription;
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new, empty pipeline with no nodes.
    /// </summary>
    public Pipeline()
    {
    }

    /// <summary>
    /// Initializes a new pipeline with the specified nodes.
    /// </summary>
    public Pipeline(IEnumerable<IPipelineNode<TState>> nodes)
        : this(null, nodes, null!)
    {
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state, nodes, and output handler.
    /// </summary>
    public Pipeline(TState defaultState, IEnumerable<IPipelineNode<TState>> nodes, Action<TState> outputHandlerAction)
        : this(null, defaultState, nodes, outputHandlerAction, null!)
    {
    }

    /// <summary>
    /// Initializes a new pipeline with the specified nodes.
    /// </summary>
    public Pipeline(params IPipelineNode<TState>[] nodes)
    {
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state and nodes.
    /// </summary>
    public Pipeline(TState defaultState, params IPipelineNode<TState>[] nodes)
    {
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }

        SetDefault(defaultState);
    }
    
    /// <summary>
    /// Initializes a new, empty pipeline with an optional name and logger.
    /// </summary>
    public Pipeline(string? name, ILogger<Pipeline<TState>> logger)
    {
        Name = name;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new pipeline with the specified nodes and an optional name and logger.
    /// </summary>
    public Pipeline(string? name, IEnumerable<IPipelineNode<TState>> nodes, ILogger<Pipeline<TState>> logger)
    {
        Name = name;
        _logger = logger;
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state, nodes, output handler, and an optional name and logger.
    /// </summary>
    public Pipeline(string? name, TState defaultState, IEnumerable<IPipelineNode<TState>> nodes, Action<TState> outputHandlerAction, ILogger<Pipeline<TState>> logger)
    {
        Name = name;
        _logger = logger;
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }

        SetDefault(defaultState);
        SetOutputHandler(outputHandlerAction);
    }

    /// <summary>
    /// Name of the pipeline (used for logging).
    /// </summary>
    public string? Name { get; set; }

    private string LogPrefix => Name == null ? "" : $"{Name}: ";

    /// <inheritdoc />
    public IPipeline<TState> SetDefault(TState state)
    {
        Input = state;
        return this;
    }

    /// <summary>
    /// Registers a new node in the pipeline. The node will be created using the type's parameterless constructor.
    /// </summary>
    public virtual IPipeline<TState> RegisterNode<TNode>() where TNode : IPipelineNode<TState>
    {
        return RegisterNode((TNode)Activator.CreateInstance(typeof(TNode))!);
    }

    /// <inheritdoc />
    public IPipeline<TState> RegisterNode(IPipelineNode<TState> node)
    {
        _logger?.LogTrace($"{LogPrefix}Registering [Node {_nodes.Count}] ({node}).");

        _subscription?.Dispose(); // Dispose old subscription if any.
        
        if (_nodes.Any())
        {
            var previousNode = _nodes.Last();
            var sourceIndex = _nodes.Count - 1;
            var destinationIndex = _nodes.Count;
            previousNode.OnNewOutput.Subscribe(output =>
            {
                lock (_lock)
                {
                    _logger?.LogTrace(
                        $"{LogPrefix}[Node {sourceIndex}] ({previousNode}) passed value [{output?.ToString() ?? "NULL"}] to [Node {destinationIndex}] ({node}).");
                    node.Input = output;
                }
            });

            _logger?.LogTrace($"{LogPrefix}Passing [Node {sourceIndex}] ({previousNode}) value [{previousNode.Output?.ToString() ?? "NULL"}] to [Node {destinationIndex}] ({node}).");
            node.Input = previousNode.Output;
        }
        else
        {
            node.Input = Input;
        }
        _nodes.Add(node);

        /*
         * We register to the output of the node AFTER we passed the input of the previous node to it.
         * If the new node sets its output in the same thread as the input is set, we don't set the pipeline output twice (once in this subscription and once at the end of the method).
         * As we don't know the behaviour of the node, there is no guarantee that this event is fired when the input is set, for this reason we set the pipeline output to the node's output manually at the end of this method.
         */
        var nodeIndex = _nodes.Count - 1;
        _subscription = node.OnNewOutput.Subscribe(o =>
        {
            lock (_lock)
            {
                _logger?.LogTrace(
                    $"{LogPrefix}[Node {nodeIndex}] ({node}) passed value [{o?.ToString() ?? "NULL"}] to pipeline output.");
                SetOutputAndCallActionWhenApplicable(o);
            }
        });

        var newOutput = node.Output;
        _logger?.LogTrace($"{LogPrefix}[Node {_nodes.Count - 1}] ({node}) registered and passed value [{newOutput?.ToString() ?? "NULL"}] to pipeline output.");
        SetOutputAndCallActionWhenApplicable(newOutput);

        return this;
    }

    /// <inheritdoc />
    public IPipeline<TState> SetOutputHandler(Action<TState> action, bool callActionDistinct = true)
    {
        _logger?.LogTrace(callActionDistinct
            ? $"{LogPrefix}Setting output handler."
            : $"{LogPrefix}Setting output handler. Action calls with duplicate values are allowed.");

        _callActionDistinct = callActionDistinct;
        _action = action;
        if (Output != null)
        {
            _action(Output);
            _logger?.LogDebug($"{LogPrefix}Action executed with current output [{Output?.ToString() ?? "NULL"}].");
        }
        else
        {
            _logger?.LogTrace($"{LogPrefix}No output value to execute.");
        }

        return this;
    }

    /// <inheritdoc />
    protected override void InputReceived(TState? state)
    {
        if (!_nodes.Any())
        {
            _logger?.LogTrace($"{LogPrefix}Input set to [{state?.ToString() ?? "NULL"}]. No nodes registered, passing to pipeline output immediately.");
            SetOutputAndCallActionWhenApplicable(Input);
            return;
        }

        var firstNode = _nodes.First();
        _logger?.LogTrace($"{LogPrefix}Input set to [{state?.ToString() ?? "NULL"}]. Passing input to first [Node 0] ({firstNode}).");
        firstNode.Input = Input;
    }

    private void SetOutputAndCallActionWhenApplicable(TState? output)
    {
        var outputChanged = !EqualityComparer<TState>.Default.Equals(Output, output);

        Output = output;
        if (_action == null)
        {
            _logger?.LogTrace($"{LogPrefix}No action set to execute.");
            return;
        }
        if (output == null)
        {
            _logger?.LogTrace($"{LogPrefix}No output value to execute.");
            return;
        }

        if (_callActionDistinct && !outputChanged)
        {
            _logger?.LogTrace($"{LogPrefix}No action executed as output has not changed.");
            return;
        }

        // Note that _action will be called AFTER OnNewOutput.
        _action.Invoke(output);
        _logger?.LogDebug($"{LogPrefix}Action executed with output [{Output?.ToString() ?? "NULL"}].");
    }

    /// <inheritdoc />
    public virtual async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (var node in _nodes)
        {
            switch (node)
            {
                case IAsyncDisposable asyncDisposable:
                    try
                    {
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"{LogPrefix}Exception when trying to dispose {node}.");
                    }
                    break;
                case IDisposable disposable:
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"{LogPrefix}Exception when trying to dispose {node}.");
                    }
                    break;
            }
        }
    }
}
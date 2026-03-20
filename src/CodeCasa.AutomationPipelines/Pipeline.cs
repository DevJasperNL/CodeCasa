using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodeCasa.AutomationPipelines;

/// <summary>
/// Represents a pipeline of nodes.
/// </summary>
public class Pipeline<TState> : PipelineNode<TState>, IPipeline<TState>
{
    private readonly Lock _lock = new();
    private readonly List<IPipelineNode<TState>> _nodes = new();
    private readonly Subject<PipelineTelemetry<TState>> _telemetrySubject = new();

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
    {
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }
    }

    /// <summary>
    /// Initializes a new pipeline with the specified default state, nodes, and output handler.
    /// </summary>
    public Pipeline(TState defaultState, IEnumerable<IPipelineNode<TState>> nodes, Action<TState> outputHandlerAction)
    {
        foreach (var node in nodes)
        {
            RegisterNode(node);
        }

        SetDefault(defaultState);
        SetOutputHandler(outputHandlerAction);
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

    /// <inheritdoc />
    public IReadOnlyCollection<IPipelineNode<TState>> Nodes => _nodes.AsReadOnly();

    /// <inheritdoc />
    public IObservable<PipelineTelemetry<TState>> Telemetry => _telemetrySubject.AsObservable();

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
                    _telemetrySubject.OnNext(new PipelineTelemetry<TState>(
                        sourceIndex,
                        previousNode.ToString(),
                        destinationIndex,
                        node.ToString(),
                        previousNode.Output
                    ));
                    node.Input = output;
                }
            });

            _telemetrySubject.OnNext(new PipelineTelemetry<TState>(
                sourceIndex,
                previousNode.ToString(),
                destinationIndex,
                node.ToString(),
                previousNode.Output
            ));
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
                _telemetrySubject.OnNext(new PipelineTelemetry<TState>(
                    nodeIndex,
                    node.ToString(),
                    null, null,
                    o
                ));

                SetOutputAndCallActionWhenApplicable(o);
            }
        });

        var newOutput = node.Output;
        _telemetrySubject.OnNext(new PipelineTelemetry<TState>(
            _nodes.Count - 1,
            node.ToString(),
            null, null,
            newOutput
        ));
        SetOutputAndCallActionWhenApplicable(newOutput);

        return this;
    }

    /// <inheritdoc />
    public IPipeline<TState> SetOutputHandler(Action<TState> action, bool callActionDistinct = true)
    {
        _callActionDistinct = callActionDistinct;
        _action = action;
        if (Output != null)
        {
            _action(Output);
        }

        return this;
    }

    /// <inheritdoc />
    protected override void InputReceived(TState? state)
    {
        if (!_nodes.Any())
        {
            _telemetrySubject.OnNext(new PipelineTelemetry<TState>(
                null, null, null, null,
                Input
            ));
            SetOutputAndCallActionWhenApplicable(Input);
            return;
        }

        var firstNode = _nodes.First();
        _telemetrySubject.OnNext(new PipelineTelemetry<TState>(
            null, null, 
            0, firstNode.ToString(),
            Input
        ));
        firstNode.Input = Input;
    }

    private void SetOutputAndCallActionWhenApplicable(TState? output)
    {
        var outputChanged = !EqualityComparer<TState>.Default.Equals(Output, output);

        Output = output;
        if (_action == null || output == null)
        {
            return;
        }
        if (_callActionDistinct && !outputChanged)
        {
            return;
        }

        // Note that _action will be called AFTER OnNewOutput.
        _action.Invoke(output);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        await base.DisposeAsync();

        _telemetrySubject.OnCompleted();
        _telemetrySubject.Dispose();

        foreach (var node in _nodes)
        {
            await node.DisposeAsync().ConfigureAwait(false);
        }
    }
}
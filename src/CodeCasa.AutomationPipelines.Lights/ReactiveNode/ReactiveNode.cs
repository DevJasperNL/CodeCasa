using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.AutomationPipelines.Lights.Utils;
using CodeCasa.Lights;
using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// A pipeline node that dynamically switches between different child nodes based on an observable source.
/// The active node can change at runtime, allowing for reactive behavior switching.
/// </summary>
public class ReactiveNode : PipelineNode<LightTransition>
{
    private readonly string? _name;
    private readonly ILogger<ReactiveNode>? _logger;
    private readonly IEqualityComparer<LightTransition>? _equalityComparer;
    private readonly Subject<Unit> _nodeChangedSubject = new();
    private readonly ConcurrentQueue<Action> _stateQueue = new();
    private int _drainingThreadId;
    private volatile bool _isDisposed;
    private IDisposable? _nodeObservableSubscription;
    private IDisposable? _activeNodeSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveNode"/> class.
    /// </summary>
    /// <param name="nodeObservable">An observable that emits the pipeline nodes to activate. Null values deactivate the current node.</param>
    /// <param name="equalityComparer">Optional equality comparer used to determine whether the output has changed. When <see langword="null"/>, the output is always set when a new value is received.</param>
    public ReactiveNode(IObservable<IPipelineNode<LightTransition>?> nodeObservable, IEqualityComparer<LightTransition>? equalityComparer = null) :
        this(null, nodeObservable, null!, equalityComparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveNode"/> class.
    /// </summary>
    /// <param name="name">Optional name for the reactive node, used for logging purposes.</param>
    /// <param name="nodeObservable">An observable that emits the pipeline nodes to activate. Null values deactivate the current node.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <param name="equalityComparer">Optional equality comparer used to determine whether the output has changed. When <see langword="null"/>, the output is always set when a new value is received.</param>
    public ReactiveNode(string? name, IObservable<IPipelineNode<LightTransition>?> nodeObservable, ILogger<ReactiveNode> logger, IEqualityComparer<LightTransition>? equalityComparer = null)
    {
        _name = name;
        _logger = logger;
        _equalityComparer = equalityComparer;
        PassThrough = true;

        _nodeObservableSubscription = nodeObservable
            .Subscribe(n =>
            {
                EnqueueStateChange(() =>
                {
                    if (n == null)
                    {
                        DeactivateActiveNode();
                        PassThrough = true;
                        _logger?.LogTrace($"{LogPrefix}No active node. Passing through data.");
                    }
                    else
                    {
                        ActivateNode(n);
                    }

                    _nodeChangedSubject.OnNext(Unit.Default);
                });
            }, error =>
            {
                // Without an error handler Rx rethrows on the producer thread and the node silently stops reacting.
                // Fall back to pass-through so the rest of the pipeline keeps working.
                _logger?.LogError(error, $"{LogPrefix}Node source failed. Deactivating and passing through data.");
                EnqueueStateChange(() =>
                {
                    DeactivateActiveNode();
                    PassThrough = true;
                    _nodeChangedSubject.OnNext(Unit.Default);
                });
            });
    }

    /// <summary>
    /// Gets the currently active pipeline node, or <see langword="null"/> if no node is active.
    /// </summary>
    public IPipelineNode<LightTransition>? ActiveNode { get; private set; }
    private string LogPrefix => _name == null ? "" : $"{_name}: ";

    /// <summary>
    /// Gets an observable that emits whenever the active node changes.
    /// </summary>
    public IObservable<Unit> NodeChanged => _nodeChangedSubject.AsObservable();

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input)
    {
        EnqueueStateChange(() =>
        {
            if (ActiveNode != null)
            {
                ActiveNode.Input = input;
            }
        });
    }

    /*
     * All state mutation is serialised through this queue. Unlike a lock, a caller on another thread never waits:
     * it enqueues and returns, and the thread that is already draining runs the action. Holding a lock while calling
     * into child nodes deadlocked nested reactive nodes (outer input vs. inner trigger, see 0b4571e for the same
     * problem in Pipeline). Actions enqueued from within a running action execute inline, which keeps synchronous
     * emissions during activation ordered exactly as before.
     */
    private void EnqueueStateChange(Action action)
    {
        if (_isDisposed)
        {
            return;
        }

        var currentThreadId = Environment.CurrentManagedThreadId;
        if (Volatile.Read(ref _drainingThreadId) == currentThreadId)
        {
            RunStateChange(action);
            return;
        }

        _stateQueue.Enqueue(action);
        while (!_stateQueue.IsEmpty && Interlocked.CompareExchange(ref _drainingThreadId, currentThreadId, 0) == 0)
        {
            try
            {
                while (_stateQueue.TryDequeue(out var next))
                {
                    RunStateChange(next);
                }
            }
            finally
            {
                Volatile.Write(ref _drainingThreadId, 0);
            }
        }
    }

    private void RunStateChange(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            // A throwing child node or downstream handler must not take the reactive node, or the trigger that fed it, down with it.
            _logger?.LogError(e, $"{LogPrefix}Processing a state change failed.");
        }
    }

    private void DeactivateActiveNode()
    {
        _activeNodeSubscription?.Dispose();
        if (ActiveNode != null)
        {
            ActiveNode.Input = null;
            ActiveNode.DisposeOrDisposeAsync().GetAwaiter().GetResult();
        }

        ActiveNode = null;
        _activeNodeSubscription = null;
    }

    private void ActivateNode(IPipelineNode<LightTransition> node)
    {
        DeactivateActiveNode();
        ActiveNode = node;
        _logger?.LogTrace($"{LogPrefix}Activating {node}.");

        // Subscribe before pushing the input so an output produced on another thread in between is not lost.
        var outputReceived = false;
        _activeNodeSubscription = node.OnNewOutput.Subscribe(output =>
        {
            outputReceived = true;
            EnqueueStateChange(() =>
            {
                // An output queued by a node that has since been replaced must not overwrite the new node's output.
                if (ReferenceEquals(ActiveNode, node))
                {
                    UpdateOutput(output);
                }
            });
        });
        node.Input = Input;
        if (!outputReceived)
        {
            UpdateOutput(node.Output);
        }
        PassThrough = false;
    }

    private void UpdateOutput(LightTransition? newOutput)
    {
        if (_equalityComparer == null || !_equalityComparer.Equals(Output, newOutput))
        {
            Output = newOutput;
        }
    }

    /// <inheritdoc />
    public override string ToString() => _name ?? base.ToString();

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        _nodeObservableSubscription?.Dispose();
        _nodeObservableSubscription = null;
        _activeNodeSubscription?.Dispose();
        _activeNodeSubscription = null;

        if (ActiveNode != null)
        {
            await ActiveNode.DisposeOrDisposeAsync();
        }

        _stateQueue.Clear();
        _nodeChangedSubject.Dispose();

        await base.DisposeAsync();
    }
}
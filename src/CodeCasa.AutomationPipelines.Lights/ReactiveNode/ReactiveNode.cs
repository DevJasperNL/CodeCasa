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
    private readonly Lock _lock = new();
    private readonly string? _name;
    private readonly ILogger<ReactiveNode>? _logger;
    private readonly Subject<Unit> _nodeChangedSubject = new();
    private IDisposable? _activeNodeSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveNode"/> class.
    /// </summary>
    /// <param name="nodeObservable">An observable that emits the pipeline nodes to activate. Null values deactivate the current node.</param>
    public ReactiveNode(IObservable<IPipelineNode<LightTransition>?> nodeObservable) :
        this(null, nodeObservable, null!)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveNode"/> class.
    /// </summary>
    /// <param name="name">Optional name for the reactive node, used for logging purposes.</param>
    /// <param name="nodeObservable">An observable that emits the pipeline nodes to activate. Null values deactivate the current node.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public ReactiveNode(string? name, IObservable<IPipelineNode<LightTransition>?> nodeObservable, ILogger<ReactiveNode> logger)
    {
        _name = name;
        _logger = logger;
        PassThrough = true;

        nodeObservable
            .Subscribe(n =>
            {
                lock (_lock)
                {
                    if (n == null)
                    {
                        DeactivateActiveNode();
                        PassThrough = true;
                        _logger?.LogTrace($"{LogPrefix}No active node. Passing through data.");
                        _nodeChangedSubject.OnNext(Unit.Default);
                        return;
                    }

                    ActivateNode(n);
                    _nodeChangedSubject.OnNext(Unit.Default);
                }
            });
    }

    public IPipelineNode<LightTransition>? ActiveNode { get; private set; }
    private string LogPrefix => _name == null ? "" : $"{_name}: ";

    /// <summary>
    /// Gets an observable that emits whenever the active node changes.
    /// </summary>
    public IObservable<Unit> NodeChanged => _nodeChangedSubject.AsObservable();

    /// <inheritdoc />
    protected override void InputReceived(LightTransition? input)
    {
        if (ActiveNode == null)
        {
            return;
        }
        lock (_lock)
        {
            if (ActiveNode != null)
            {
                ActiveNode.Input = input;
            }
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
        // todo: move after input setting?
        _activeNodeSubscription = ActiveNode.OnNewOutput.Subscribe(output =>
        {
            if (EqualityComparer<LightTransition>.Default.Equals(Output, output))
            {
                return;
            }
            lock (_lock)
            {
                if (!EqualityComparer<LightTransition>.Default.Equals(Output, output))
                {
                    Output = output;
                }
            }
        });
        ActiveNode.Input = Input;
        if (!EqualityComparer<LightTransition>.Default.Equals(Output, ActiveNode.Output))
        {
            Output = ActiveNode.Output;
        }
        PassThrough = false;
    }

    /// <inheritdoc />
    public override string ToString() => _name ?? base.ToString();
}
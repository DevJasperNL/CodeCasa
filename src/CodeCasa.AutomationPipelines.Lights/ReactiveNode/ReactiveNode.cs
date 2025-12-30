using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.AutomationPipelines.Lights.Utils;
using CodeCasa.Lights;
using Microsoft.Extensions.Logging;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

public class ReactiveNode : PipelineNode<LightTransition>
{
    private readonly Lock _lock = new();
    private readonly string? _name;
    private readonly ILogger<ReactiveNode>? _logger;
    private readonly Subject<Unit> _nodeChangedSubject = new();
    private IPipelineNode<LightTransition>? _activeNode;
    private IDisposable? _activeNodeSubscription;

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

    private string LogPrefix => _name == null ? "" : $"{_name}: ";

    public IObservable<Unit> NodeChanged => _nodeChangedSubject.AsObservable();

    protected override void InputReceived(LightTransition? input)
    {
        if (_activeNode == null)
        {
            return;
        }
        lock (_lock)
        {
            if (_activeNode != null)
            {
                _activeNode.Input = input;
            }
        }
    }

    private void DeactivateActiveNode()
    {
        _activeNodeSubscription?.Dispose();
        if (_activeNode != null)
        {
            _activeNode.Input = null;
            _activeNode.DisposeOrDisposeAsync().GetAwaiter().GetResult();
        }

        _activeNode = null;
        _activeNodeSubscription = null;
    }

    private void ActivateNode(IPipelineNode<LightTransition> node)
    {
        DeactivateActiveNode();
        _activeNode = node;
        _logger?.LogTrace($"{LogPrefix}Activating {node}.");
        // todo: move after input setting?
        _activeNodeSubscription = _activeNode.OnNewOutput.Subscribe(output =>
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
        _activeNode.Input = Input;
        if (!EqualityComparer<LightTransition>.Default.Equals(Output, _activeNode.Output))
        {
            Output = _activeNode.Output;
        }
        PassThrough = false;
    }

    public override string ToString() => _name ?? base.ToString();
}
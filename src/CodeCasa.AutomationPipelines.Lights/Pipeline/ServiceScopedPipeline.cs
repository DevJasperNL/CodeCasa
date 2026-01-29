using CodeCasa.AutomationPipelines.Lights.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Pipeline;

internal sealed class ServiceScopedPipeline<TNode>(IServiceScope scope, IPipeline<TNode> pipeline)
    : IPipeline<TNode>, IDisposable, IAsyncDisposable
{
    private readonly IServiceScope _scope = scope ?? throw new ArgumentNullException(nameof(scope));
    private readonly IPipeline<TNode> _instance = pipeline ?? throw new ArgumentNullException(nameof(pipeline));

    public void Dispose()
    {
        (_instance as IDisposable)?.Dispose();
        _scope.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _instance.DisposeOrDisposeAsync();
        await _scope.DisposeOrDisposeAsync();
    }

    public TNode? Input
    {
        get => _instance.Input;
        set => _instance.Input = value;
    }

    public TNode? Output => _instance.Output;

    public IObservable<TNode?> OnNewOutput => _instance.OnNewOutput;

    public IPipeline<TNode> SetDefault(TNode state)
    {
        _instance.SetDefault(state);
        return this;
    }

    public IPipeline<TNode> RegisterNode<TNode1>() where TNode1 : IPipelineNode<TNode>
    {
        _instance.RegisterNode<TNode1>();
        return this;
    }

    public IPipeline<TNode> RegisterNode(IPipelineNode<TNode> node)
    {
        _instance.RegisterNode(node);
        return this;
    }

    public IPipeline<TNode> SetOutputHandler(Action<TNode> action, bool callActionDistinct = true)
    {
        _instance.SetOutputHandler(action, callActionDistinct);
        return this;
    }
}
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines;

/// <summary>
/// Represents a pipeline of nodes.
/// This pipeline implementation can use the service provider to resolve nodes.
/// </summary>
public class ServiceProviderPipeline<TState> : Pipeline<TState>
{
    private readonly IServiceScope _serviceScope;
    private readonly IServiceProvider _serviceProvider;
    private bool _isDisposed;

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, IEnumerable<IPipelineNode<TState>> nodes)
        : base(nodes)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(
        IServiceProvider serviceProvider,
        TState defaultState,
        IEnumerable<IPipelineNode<TState>> nodes,
        Action<TState> outputHandlerAction)
        : base(defaultState, nodes, outputHandlerAction)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, params IPipelineNode<TState>[] nodes)
        : base(nodes)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
    }

    /// <inheritdoc />
    public ServiceProviderPipeline(IServiceProvider serviceProvider, TState defaultState, params IPipelineNode<TState>[] nodes)
        : base(defaultState, nodes)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
    }

    /// <summary>
    /// Registers a new node in the pipeline. The node will be created using the service provider.
    /// </summary>
    public override IPipeline<TState> RegisterNode<TNode>()
    {
        return RegisterNode(ActivatorUtilities.CreateInstance<TNode>(_serviceProvider));
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await base.DisposeAsync();

        if (_serviceScope is IAsyncDisposable asyncScope)
        {
            await asyncScope.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _serviceScope.Dispose();
        }
    }
}
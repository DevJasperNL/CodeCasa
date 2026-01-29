using CodeCasa.AutomationPipelines.Lights.Utils;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.ReactiveNode;

/// <summary>
/// A pipeline that combines a reactive node with a reactive dimmer node for managing light transitions.
/// </summary>
internal sealed class ReactiveDimmerPipeline : Pipeline<LightTransition>
{
    private readonly IServiceScope _scope;
    private readonly IRegisterInterface<ReactiveDimmerPipeline> _registerInterface;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveDimmerPipeline"/> class.
    /// </summary>
    public ReactiveDimmerPipeline(
        IServiceScope scope,
        ReactiveNode reactiveNode, 
        ReactiveDimmerNode reactiveDimmerNode,
        IRegisterInterface<ReactiveDimmerPipeline> registerInterface)
    {
        _scope = scope;
        _registerInterface = registerInterface;
        _registerInterface.Register(this);
        RegisterNode(reactiveNode);
        RegisterNode(reactiveDimmerNode);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        _registerInterface.Unregister(this);
        await base.DisposeAsync();
        await _scope.DisposeOrDisposeAsync();
    }
}
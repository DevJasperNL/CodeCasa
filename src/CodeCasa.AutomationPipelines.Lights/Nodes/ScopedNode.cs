using CodeCasa.AutomationPipelines.Lights.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    internal class ScopedNode<TState>(IServiceScope serviceScope, IPipelineNode<TState> innerNode)
        : IPipelineNode<TState>, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await serviceScope.DisposeOrDisposeAsync();
            await innerNode.DisposeOrDisposeAsync();
        }

        public TState? Input
        {
            get => innerNode.Input;
            set => innerNode.Input = value;
        }

        public TState? Output => innerNode.Output;
        public IObservable<TState?> OnNewOutput => innerNode.OnNewOutput;

        public override string? ToString() => $"{innerNode} (scoped)";
    }
}

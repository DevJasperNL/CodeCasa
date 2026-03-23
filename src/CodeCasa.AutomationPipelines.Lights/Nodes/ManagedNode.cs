using CodeCasa.AutomationPipelines.Lights.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Nodes
{
    internal class ManagedNode<TState>(IServiceScope serviceScope, IPipelineNode<TState> innerNode)
        : IPipelineNode<TState>, IAsyncDisposable
    {
        public Guid Id => innerNode.Id;
        public string? Name
        {
            get => innerNode.Name;
            set => innerNode.Name = value;
        }

        public TState? Input
        {
            get => innerNode.Input;
            set => innerNode.Input = value;
        }

        public TState? Output => innerNode.Output;
        public IObservable<TState?> OnNewOutput => innerNode.OnNewOutput;

        public override string? ToString() => $"{innerNode} (scoped)";

        public async ValueTask DisposeAsync()
        {
            await serviceScope.DisposeOrDisposeAsync();
            await innerNode.DisposeOrDisposeAsync();
        }
    }
}

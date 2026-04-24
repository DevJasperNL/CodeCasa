using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class PipelineNodeFactoryExtensions
    {
        public static ManagedNode<LightTransition> CreateScopedNode(
            this Func<IServiceProvider, IPipelineNode<LightTransition>> factory, 
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            return new ManagedNode<LightTransition>(scope, factory(scope.ServiceProvider));
        }

        public static ManagedNode<LightTransition>? CreateScopedNodeOrNull(
            this Func<IServiceProvider, IPipelineNode<LightTransition>?> factory,
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var node = factory(scope.ServiceProvider);
            if (node != null)
            {
                return new ManagedNode<LightTransition>(scope, node);
            }
            scope.Dispose();
            return null;
        }
    }
}

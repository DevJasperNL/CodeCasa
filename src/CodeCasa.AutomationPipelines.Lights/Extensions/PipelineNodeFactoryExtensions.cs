using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class PipelineNodeFactoryExtensions
    {
        public static ScopedPipelineNode<LightTransition> CreateScopedNode(
            this Func<IServiceProvider, IPipelineNode<LightTransition>> factory, 
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            return new ScopedPipelineNode<LightTransition>(factory(scope.ServiceProvider), scope);
        }

        public static ScopedPipelineNode<LightTransition>? CreateScopedNodeOrNull(
            this Func<IServiceProvider, IPipelineNode<LightTransition>?> factory,
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var node = factory(scope.ServiceProvider);
            if (node != null)
            {
                return new ScopedPipelineNode<LightTransition>(node, scope);
            }
            scope.Dispose();
            return null;
        }
    }
}

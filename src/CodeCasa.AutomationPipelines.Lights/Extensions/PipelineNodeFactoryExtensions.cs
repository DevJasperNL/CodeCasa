using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class PipelineNodeFactoryExtensions
    {
        public static ServiceScopedNode<LightTransition> CreateScopedNode(
            this Func<IServiceProvider, IPipelineNode<LightTransition>> factory, 
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            return new ServiceScopedNode<LightTransition>(scope, factory(scope.ServiceProvider));
        }

        public static ServiceScopedNode<LightTransition>? CreateScopedNodeOrNull(
            this Func<IServiceProvider, IPipelineNode<LightTransition>?> factory,
            IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            var node = factory(scope.ServiceProvider);
            if (node != null)
            {
                return new ServiceScopedNode<LightTransition>(scope, node);
            }
            scope.Dispose();
            return null;
        }
    }
}

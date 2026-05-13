using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace CodeCasa.AutomationPipelines.Lights.Extensions
{
    internal static class LightTransitionReactiveNodeConfiguratorExtensions
    {
        public static ILightTransitionReactiveNodeConfigurator<T> HandleExternalLightStateChanges<T>(
            this ILightTransitionReactiveNodeConfigurator<T> configurator) where T : ILight
        {
            configurator.AddNodeSource(sp =>
            {
                var light = sp.GetRequiredService<ILight>();
                var context = sp.GetRequiredService<LightPipelineContext>();
                return light.StateChanges()
                    .Where(l => 
                        l.New?.Brightness == 0 && 
                        (context.State == null || 
                         context.State.Output?.LightParameters.Brightness != 0))
                    .Select(_ => (Func<IServiceProvider, IPipelineNode<LightTransition>?>)(_ => new TurnOffThenPassThroughNode()));
            });

            return configurator;
        }
    }
}

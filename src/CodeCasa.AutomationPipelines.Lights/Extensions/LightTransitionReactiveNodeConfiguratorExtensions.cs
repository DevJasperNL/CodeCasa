using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

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
                return light.StateChanges().Where(l =>
                {
                    // todo: make sure we did not set the brightness to 0 ourselves
                    return l.New?.Brightness == 0;
                }).Select(_ => (Func<IServiceProvider, IPipelineNode<LightTransition>?>)(_ => new TurnOffThenPassThroughNode()));
            });

            return configurator;
        }
    }
}

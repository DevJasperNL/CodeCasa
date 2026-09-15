using CodeCasa.AutomationPipelines.Lights.Nodes;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.AutomationPipelines.Lights.ReactiveNode;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace CodeCasa.AutomationPipelines.Lights.Extensions;

internal static class LightTransitionReactiveNodeConfiguratorExtensions
{
    public static ILightTransitionReactiveNodeConfigurator<T> HandleExternalLightStateChanges<T>(
        this ILightTransitionReactiveNodeConfigurator<T> configurator, InteractionNodeOptions options) where T : ILight
    {
        configurator.AddNodeSource(sp =>
        {
            var light = sp.GetRequiredService<ILight>();
            var context = sp.GetService<LightPipelineContext>() ?? throw new InvalidOperationException($"{nameof(HandleExternalLightStateChanges)} can only be applies to reactive nodes hosted in a pipeline.");
            var stateChanges = light.StateChanges();

            var externalOff = stateChanges
                .Where(l =>
                    l.New?.Brightness == 0 &&
                    (context.State == null ||
                     context.State.Output?.LightParameters.Brightness != 0))
                .Select(_ => (Func<IServiceProvider, IPipelineNode<LightTransition>?>)(_ => new TurnOffThenPassThroughNode()));
            if (!options.HoldExternalChanges)
            {
                return externalOff;
            }

            var scheduler = sp.GetRequiredService<IScheduler>();
            LightParameters? lastHeldParameters = null;
            var externalChanges = stateChanges
                .Select(l => l.New)
                .Where(parameters => parameters != null && (parameters.Brightness ?? 0) != 0 && IsExternalChange(parameters))
                .Select(parameters =>
                {
                    lastHeldParameters = parameters;
                    return (Func<IServiceProvider, IPipelineNode<LightTransition>?>)(_ => new HoldExternalChangeNode(parameters!, options.HoldTimeout, scheduler));
                });
            return externalOff.Merge(externalChanges);

            bool IsExternalChange(LightParameters? parameters)
            {
                var state = context.State;
                if (state?.Output == null)
                {
                    return true;
                }
                if (options.Comparer.Equals(state.Output.LightParameters, parameters))
                {
                    // The light reports what the pipeline sent.
                    return false;
                }

                // A held external change is already what the light shows, so a further change right after it is external too.
                var outputIsHeldChange = lastHeldParameters != null && options.Comparer.Equals(state.Output.LightParameters, lastHeldParameters);
                var settledAt = state.OutputSetAt + (state.Output.TransitionTime ?? TimeSpan.Zero) + options.SettleTime;
                return outputIsHeldChange || scheduler.Now >= settledAt;
            }
        });

        return configurator;
    }
}

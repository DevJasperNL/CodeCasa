using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Notifications.Lights.Extensions
{
    /// <summary>
    /// Extension methods for configuring light transition pipelines with notifications.
    /// </summary>
    public static class LightTransitionPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Adds light notifications to the pipeline.
        /// </summary>
        /// <typeparam name="TLight">The type of light being controlled.</typeparam>
        /// <param name="configurator">The pipeline configurator.</param>
        /// <returns>The updated pipeline configurator.</returns>
        public static ILightTransitionPipelineConfigurator<TLight> AddNotifications<TLight>(
            this ILightTransitionPipelineConfigurator<TLight> configurator)
            where TLight : ILight
        {
            return configurator.AddReactiveNode(c =>
            {
                c
                    .SetName("Notifications")
                    .AddNodeSource(sp =>
                    {
                        var context = sp.GetService<LightNotificationManagerContext>();

                        if (context is null)
                        {
                            throw new InvalidOperationException(
                                $"Could not resolve {nameof(LightNotificationManagerContext)}. " +
                                $"Make sure to call {nameof(ServiceCollectionExtensions.AddLightNotifications)} " +
                                $"on your {nameof(IServiceCollection)} before using {nameof(AddNotifications)}.");
                        }
                        return context.LightNotifications
                            .Select(lnc => lnc == null ? _ => null : lnc.CreateFactory());
                    });
            });
        }
    }
}

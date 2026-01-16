using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.AutomationPipelines.Lights.Pipeline;
using CodeCasa.Lights;
using System.Reactive.Linq;

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
        /// <param name="lightNotificationManagerContext">The context providing light notifications.</param>
        /// <returns>The updated pipeline configurator.</returns>
        public static ILightTransitionPipelineConfigurator<TLight> AddNotifications<TLight>(
            this ILightTransitionPipelineConfigurator<TLight> configurator, LightNotificationManagerContext lightNotificationManagerContext)
            where TLight : ILight
        {
            return configurator.AddReactiveNode(c =>
            {
                c.AddNodeSource(lightNotificationManagerContext.LightNotifications.Select(lnc =>
                {
                    if (lnc == null)
                    {
                        return new Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>?>(_ => null);
                    }

                    if (lnc.NodeFactory != null)
                    {
                        return ctx => lnc.NodeFactory(ctx);
                    }

                    if (lnc.NodeType == null)
                    {
                        throw new InvalidOperationException("Both NodeFactory and NodeType are null.");
                    }

                    return ctx => (IPipelineNode<LightTransition>)ctx.ServiceProvider.CreateInstanceWithinContext(lnc.NodeType, ctx);
                }));
            });
        }
    }
}

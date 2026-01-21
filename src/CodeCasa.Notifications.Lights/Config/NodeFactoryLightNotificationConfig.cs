using CodeCasa.AutomationPipelines;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;

namespace CodeCasa.Notifications.Lights.Config;

/// <summary>
/// A light notification configuration that creates pipeline nodes using a provided factory function.
/// </summary>
/// <typeparam name="TLight">The type of light that this configuration is designed for, which must implement <see cref="ILight"/>.</typeparam>
public class NodeFactoryLightNotificationConfig<TLight> : ILightNotificationConfig where TLight : ILight
{
    /// <inheritdoc/>
    public int Priority { get; set; }

    /// <summary>
    /// Gets the factory function used to create pipeline nodes for the specified light type.
    /// </summary>
    public Func<IServiceProvider, IPipelineNode<LightTransition>> NodeFactory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeFactoryLightNotificationConfig{TLight}"/> class.
    /// </summary>
    /// <param name="nodeFactory">The factory function that creates pipeline nodes from a service provider.</param>
    /// <param name="priority">The priority of the notification. Higher values indicate higher priority.</param>
    internal NodeFactoryLightNotificationConfig(Func<IServiceProvider, IPipelineNode<LightTransition>> nodeFactory, int priority)
    {
        NodeFactory = nodeFactory;
        Priority = priority;
    }

    /// <inheritdoc/>
    public Func<IServiceProvider, IPipelineNode<LightTransition>?> CreateFactory()
    {
        return s =>
        {
            var light = s.GetService<TLight>();
            if (light == null)
            {
                // This can occur if the notification is applied to a pipeline with a different light type.
                // If that occurs we return regardless of the node actually requiring the light.
                return null;
            }

            return NodeFactory(s);
        };
    }
}
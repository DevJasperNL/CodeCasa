using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;

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
    public Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> NodeFactory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeFactoryLightNotificationConfig{TLight}"/> class.
    /// </summary>
    /// <param name="nodeFactory">The factory function that creates pipeline nodes from a light pipeline context.</param>
    /// <param name="priority">The priority of the notification. Higher values indicate higher priority.</param>
    internal NodeFactoryLightNotificationConfig(Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, int priority)
    {
        NodeFactory = nodeFactory;
        Priority = priority;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Returns <c>null</c> if <typeparamref name="TLight1"/> does not match <typeparamref name="TLight"/>,
    /// as the factory is only compatible with the specific light type it was configured for.
    /// </remarks>
    public Func<ILightPipelineContext<TLight1>, IPipelineNode<LightTransition>?> CreateFactory<TLight1>() where TLight1 : ILight
    {
        // Note: Types need to match to notify on this pipeline.
        if (NodeFactory is Func<ILightPipelineContext<TLight1>, IPipelineNode<LightTransition>> matchedFactory)
        {
            return matchedFactory;
        }
        return _ => null;
    }
}
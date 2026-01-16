using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;

namespace CodeCasa.Notifications.Lights.Config;

/// <summary>
/// Defines the configuration for a light notification, including its priority and the ability to create pipeline nodes.
/// </summary>
public interface ILightNotificationConfig
{
    /// <summary>
    /// Gets or sets the priority of the notification.
    /// Higher values indicate higher priority.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Creates a factory function that produces a pipeline node for the specified light type.
    /// </summary>
    /// <typeparam name="TLight">The type of light that implements <see cref="ILight"/>.</typeparam>
    /// <returns>
    /// A factory function that takes a <see cref="ILightPipelineContext{TLight}"/> and returns
    /// a <see cref="IPipelineNode{LightTransition}"/>, or <c>null</c> if the light type is not supported.
    /// </returns>
    public Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>?> CreateFactory<TLight>() where TLight : ILight;
}

/// <summary>
/// A light notification configuration that creates pipeline nodes by resolving a specific node type from the service provider.
/// </summary>
/// <typeparam name="TNode">The type of pipeline node to create, which must implement <see cref="IPipelineNode{LightTransition}"/>.</typeparam>
public class NodeTypeLightNotificationConfig<TNode> : ILightNotificationConfig where TNode : IPipelineNode<LightTransition>
{
    /// <inheritdoc/>
    public int Priority { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeTypeLightNotificationConfig{TNode}"/> class.
    /// </summary>
    /// <param name="priority">The priority of the notification. Higher values indicate higher priority.</param>
    internal NodeTypeLightNotificationConfig(int priority)
    {
        Priority = priority;
    }

    /// <inheritdoc/>
    public Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>?> CreateFactory<TLight>() where TLight : ILight
    {
        return context => (IPipelineNode<LightTransition>)context.ServiceProvider.CreateInstanceWithinContext(typeof(TNode), context);
    }
}

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
using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;

namespace CodeCasa.Notifications.Lights.Config;

/// <summary>
/// Configuration for a light notification.
/// </summary>
public class LightNotificationConfig
{
    /// <summary>
    /// Gets or sets the priority of the notification.
    /// Higher values indicate higher priority.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets the type of the pipeline node associated with the notification.
    /// </summary>
    public Type? NodeType { get; }

    /// <summary>
    /// Gets the factory function to create the pipeline node associated with the notification.
    /// </summary>
    public Func<ILightPipelineContext, IPipelineNode<LightTransition>>? NodeFactory { get; }

    internal LightNotificationConfig(Type nodeType, int priority)
    {
        NodeType = nodeType;
        Priority = priority;
    }

    internal LightNotificationConfig(Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, int priority)
    {
        NodeFactory = nodeFactory;
        Priority = priority;
    }
}
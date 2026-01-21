using CodeCasa.AutomationPipelines;
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
    /// <returns>
    /// A factory function that takes a <see cref="IServiceProvider"/> and returns
    /// a <see cref="IPipelineNode{LightTransition}"/>, or <c>null</c> if the light type is not supported.
    /// </returns>
    public Func<IServiceProvider, IPipelineNode<LightTransition>?> CreateFactory();
}
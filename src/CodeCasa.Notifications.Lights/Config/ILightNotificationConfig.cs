using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
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
using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.AutomationPipelines.Lights.Extensions;
using CodeCasa.Lights;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CodeCasa.Notifications.Lights.Config;

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
        return context => ActivatorUtilities.CreateInstance<TNode>(context.ServiceProvider);
    }
}
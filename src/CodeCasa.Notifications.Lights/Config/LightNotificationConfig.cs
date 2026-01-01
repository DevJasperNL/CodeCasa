using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;

namespace CodeCasa.Notifications.Lights.Config;

public class LightNotificationConfig
{
    public int Priority { get; set; }
    public Type? NodeType { get; }
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
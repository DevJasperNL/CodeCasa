using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights
{
    public class LightNotificationContext(LightNotificationManager lightNotificationManager)
    {
        public LightNotification Notify<TNode>(LightNotification lightNotificationToReplace) where TNode : IPipelineNode<LightTransition> => Notify<TNode>(lightNotificationToReplace, 0);

        public LightNotification Notify<TNode>(LightNotification lightNotificationToReplace, int priority) where TNode : IPipelineNode<LightTransition>
        {
            return Notify<TNode>(lightNotificationToReplace.Id, priority);
        }

        public LightNotification Notify<TNode>(string notificationId) where TNode : IPipelineNode<LightTransition> => Notify<TNode>(notificationId, 0);

        public LightNotification Notify<TNode>(string notificationId, int priority) where TNode : IPipelineNode<LightTransition>
        {
            return lightNotificationManager.Notify(new LightNotificationConfig(typeof(TNode), priority), notificationId);
        }

        public LightNotification Notify(LightNotification lightNotificationToReplace, Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory) => Notify(lightNotificationToReplace, nodeFactory, 0);

        public LightNotification Notify(LightNotification lightNotificationToReplace, Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, int priority)
        {
            return Notify(lightNotificationToReplace.Id, nodeFactory, priority);
        }

        public LightNotification Notify(string notificationId, Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory) => Notify(notificationId, nodeFactory, 0);

        public LightNotification Notify(string notificationId, Func<ILightPipelineContext, IPipelineNode<LightTransition>> nodeFactory, int priority)
        {
            return lightNotificationManager.Notify(new LightNotificationConfig(nodeFactory, priority), notificationId);
        }

        public void RemoveNotification(LightNotification notificationToRemove)
        {
            RemoveNotification(notificationToRemove.Id);
        }

        public bool RemoveNotification(string id)
        {
            return lightNotificationManager.Remove(id);
        }
    }
}

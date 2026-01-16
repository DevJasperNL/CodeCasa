using CodeCasa.AutomationPipelines;
using CodeCasa.AutomationPipelines.Lights.Context;
using CodeCasa.Lights;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights
{
    /// <summary>
    /// Context for managing light notifications.
    /// </summary>
    /// <param name="lightNotificationManager">The manager responsible for handling light notifications.</param>
    public class LightNotificationContext(LightNotificationManager lightNotificationManager)
    {
        /// <summary>
        /// Notifies with a new light notification, replacing an existing one.
        /// </summary>
        /// <typeparam name="TNode">The type of the pipeline node representing the notification behavior.</typeparam>
        /// <param name="lightNotificationToReplace">The existing light notification to replace.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TNode>(LightNotification lightNotificationToReplace) where TNode : IPipelineNode<LightTransition> => Notify<TNode>(lightNotificationToReplace, 0);

        /// <summary>
        /// Notifies with a new light notification with a specific priority, replacing an existing one.
        /// </summary>
        /// <typeparam name="TNode">The type of the pipeline node representing the notification behavior.</typeparam>
        /// <param name="lightNotificationToReplace">The existing light notification to replace.</param>
        /// <param name="priority">The priority of the notification.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TNode>(LightNotification lightNotificationToReplace, int priority) where TNode : IPipelineNode<LightTransition>
        {
            return Notify<TNode>(lightNotificationToReplace.Id, priority);
        }

        /// <summary>
        /// Notifies with a new light notification using a specific ID.
        /// </summary>
        /// <typeparam name="TNode">The type of the pipeline node representing the notification behavior.</typeparam>
        /// <param name="notificationId">The unique identifier for the notification.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TNode>(string notificationId) where TNode : IPipelineNode<LightTransition> => Notify<TNode>(notificationId, 0);

        /// <summary>
        /// Notifies with a new light notification with a specific priority and ID.
        /// </summary>
        /// <typeparam name="TNode">The type of the pipeline node representing the notification behavior.</typeparam>
        /// <param name="notificationId">The unique identifier for the notification.</param>
        /// <param name="priority">The priority of the notification.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TNode>(string notificationId, int priority) where TNode : IPipelineNode<LightTransition>
        {
            return lightNotificationManager.Notify(new NodeTypeLightNotificationConfig<TNode>(priority), notificationId);
        }

        /// <summary>
        /// Notifies with a new light notification using a factory, replacing an existing one.
        /// </summary>
        /// <param name="lightNotificationToReplace">The existing light notification to replace.</param>
        /// <param name="nodeFactory">The factory function to create the pipeline node.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TLight>(LightNotification lightNotificationToReplace, Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory) where TLight : ILight => Notify(lightNotificationToReplace, nodeFactory, 0);

        /// <summary>
        /// Notifies with a new light notification using a factory and specific priority, replacing an existing one.
        /// </summary>
        /// <param name="lightNotificationToReplace">The existing light notification to replace.</param>
        /// <param name="nodeFactory">The factory function to create the pipeline node.</param>
        /// <param name="priority">The priority of the notification.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TLight>(LightNotification lightNotificationToReplace, Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, int priority) where TLight : ILight
        {
            return Notify(lightNotificationToReplace.Id, nodeFactory, priority);
        }

        /// <summary>
        /// Notifies with a new light notification using a factory and specific ID.
        /// </summary>
        /// <param name="notificationId">The unique identifier for the notification.</param>
        /// <param name="nodeFactory">The factory function to create the pipeline node.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TLight>(string notificationId, Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory) where TLight : ILight => Notify(notificationId, nodeFactory, 0);

        /// <summary>
        /// Notifies with a new light notification using a factory, specific priority, and ID.
        /// </summary>
        /// <param name="notificationId">The unique identifier for the notification.</param>
        /// <param name="nodeFactory">The factory function to create the pipeline node.</param>
        /// <param name="priority">The priority of the notification.</param>
        /// <returns>The created or updated light notification.</returns>
        public LightNotification Notify<TLight>(string notificationId, Func<ILightPipelineContext<TLight>, IPipelineNode<LightTransition>> nodeFactory, int priority) where TLight : ILight
        {
            return lightNotificationManager.Notify(new NodeFactoryLightNotificationConfig<TLight>(nodeFactory, priority), notificationId);
        }

        /// <summary>
        /// Removes a specific light notification.
        /// </summary>
        /// <param name="notificationToRemove">The light notification to remove.</param>
        public void RemoveNotification(LightNotification notificationToRemove)
        {
            RemoveNotification(notificationToRemove.Id);
        }

        /// <summary>
        /// Removes a light notification by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to remove.</param>
        /// <returns>True if the notification was successfully removed; otherwise, false.</returns>
        public bool RemoveNotification(string id)
        {
            return lightNotificationManager.Remove(id);
        }
    }
}

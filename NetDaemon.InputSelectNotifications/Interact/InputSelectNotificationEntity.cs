
using NetDaemon.InputSelectNotifications.Config;

namespace NetDaemon.InputSelectNotifications.Interact
{
    public abstract class InputSelectNotificationEntity(IInputSelectNotificationEntity inner)
        : IInputSelectNotificationEntity
    {
        protected readonly IInputSelectNotificationEntity Inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public string InputSelectEntityId => Inner.InputSelectEntityId;
        public string? InputNumberEntityId => Inner.InputNumberEntityId;

        public virtual InputSelectNotification Notify(IInputSelectNotificationConfig notification)
        {
            return Inner.Notify(notification);
        }

        public virtual InputSelectNotification Notify(IInputSelectNotificationConfig notification, InputSelectNotification notificationToReplace)
        {
            return Inner.Notify(notification, notificationToReplace);
        }

        public virtual InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id)
        {
            return Inner.Notify(notification, id);
        }

        public virtual void RemoveNotification(InputSelectNotification notificationToRemove)
        {
            Inner.RemoveNotification(notificationToRemove);
        }

        public virtual void RemoveNotification(string id)
        {
            Inner.RemoveNotification(id);
        }
    }
}


using NetDaemon.PhoneNotifications;

namespace NetDaemon.InputSelectNotifications.Interact
{
    public interface IInputSelectNotificationEntity
    {
        InputSelectNotification Notify(InputSelectNotificationConfig notification);
        InputSelectNotification Notify(InputSelectNotificationConfig notification, InputSelectNotification notificationToReplace);
        InputSelectNotification Notify(InputSelectNotificationConfig notification, string id);
        void RemoveNotification(InputSelectNotification notificationToRemove);
        void RemoveNotification(string id);
    }
}

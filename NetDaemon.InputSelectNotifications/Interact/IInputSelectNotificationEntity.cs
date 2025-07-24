
using NetDaemon.InputSelectNotifications.Config;

namespace NetDaemon.InputSelectNotifications.Interact;

public interface IInputSelectNotificationEntity
{
    InputSelectNotification Notify(IInputSelectNotificationConfig notification);
    InputSelectNotification Notify(IInputSelectNotificationConfig notification, InputSelectNotification notificationToReplace);
    InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id);
    void RemoveNotification(InputSelectNotification notificationToRemove);
    void RemoveNotification(string id);
}
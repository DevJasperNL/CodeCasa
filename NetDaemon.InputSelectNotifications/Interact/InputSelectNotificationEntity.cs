using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NetDaemon.InputSelectNotifications.Config;

namespace NetDaemon.InputSelectNotifications.Interact;

public class InputSelectNotificationEntity : IInputSelectNotificationEntity
{
    private readonly ReplaySubject<(string id, InputSelectNotificationConfig config)> _notifySubject = new();
    private readonly ReplaySubject<string> _removeNotificationSubject = new();

    internal IObservable<(string id, InputSelectNotificationConfig config)> NotifyObservable => _notifySubject.AsObservable();
    internal IObservable<string> RemoveNotificationObservable => _removeNotificationSubject.AsObservable();

    public InputSelectNotification Notify(InputSelectNotificationConfig notification)
    {
        return Notify(notification, Guid.NewGuid().ToString());
    }

    public InputSelectNotification Notify(InputSelectNotificationConfig notification,
        InputSelectNotification notificationToReplace)
    {
        _notifySubject.OnNext((notificationToReplace.Id, notification));
        return new InputSelectNotification(notificationToReplace.Id, Disposable.Create(() => RemoveNotification(notificationToReplace.Id)));
    }

    public InputSelectNotification Notify(InputSelectNotificationConfig notification, string id)
    {
        _notifySubject.OnNext((id, notification));
        return new InputSelectNotification(id, Disposable.Create(() => RemoveNotification(id)));
    }

    public void RemoveNotification(InputSelectNotification notificationToRemove)
    {
        _removeNotificationSubject.OnNext(notificationToRemove.Id);
    }

    public void RemoveNotification(string id)
    {
        _removeNotificationSubject.OnNext(id);
    }
}
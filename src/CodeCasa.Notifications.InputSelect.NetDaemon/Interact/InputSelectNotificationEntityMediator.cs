using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.Notifications.InputSelect.NetDaemon.Config;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Interact;

internal class InputSelectNotificationEntityMediator(string inputSelectEntityId, string? inputNumberEntityId)
    : IInputSelectNotificationEntity
{
    private readonly ReplaySubject<(string id, IInputSelectNotificationConfig config)> _notifySubject = new();
    private readonly ReplaySubject<string> _removeNotificationSubject = new();

    internal IObservable<(string id, IInputSelectNotificationConfig config)> NotifyObservable => _notifySubject.AsObservable();
    internal IObservable<string> RemoveNotificationObservable => _removeNotificationSubject.AsObservable();

    public string InputSelectEntityId { get; } = inputSelectEntityId;
    public string? InputNumberEntityId { get; } = inputNumberEntityId;

    public InputSelectNotification Notify(IInputSelectNotificationConfig notification)
    {
        return Notify(notification, Guid.NewGuid().ToString());
    }

    public InputSelectNotification Notify(IInputSelectNotificationConfig notification,
        InputSelectNotification notificationToReplace)
    {
        _notifySubject.OnNext((notificationToReplace.Id, notification));
        return new InputSelectNotification(notificationToReplace.Id, Disposable.Create(() => RemoveNotification(notificationToReplace.Id)));
    }

    public InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id)
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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.Notifications.InputSelect.NetDaemon.Config;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Interact;

internal abstract record NotificationCommand(string Id);

internal sealed record NotifyCommand(string Id, IInputSelectNotificationConfig Config) : NotificationCommand(Id);

internal sealed record RemoveCommand(string Id) : NotificationCommand(Id);

internal class InputSelectNotificationEntityMediator(string inputSelectEntityId, string? inputNumberEntityId)
    : IInputSelectNotificationEntity
{
    private readonly Lock _lock = new();
    private readonly Subject<NotificationCommand> _commands = new();

    // Calls made before the background handler attaches are buffered per id (a later notify replaces an earlier one
    // for the same id, a remove drops it) so the buffer cannot grow beyond the number of distinct notifications.
    private List<NotificationCommand>? _pendingUntilFirstSubscriber = [];

    internal IObservable<NotificationCommand> Commands => Observable.Create<NotificationCommand>(observer =>
    {
        lock (_lock)
        {
            var subscription = _commands.Subscribe(observer);
            if (_pendingUntilFirstSubscriber != null)
            {
                var pending = _pendingUntilFirstSubscriber;
                _pendingUntilFirstSubscriber = null;
                foreach (var command in pending)
                {
                    observer.OnNext(command);
                }
            }
            return subscription;
        }
    });

    public string InputSelectEntityId { get; } = inputSelectEntityId;
    public string? InputNumberEntityId { get; } = inputNumberEntityId;

    public InputSelectNotification Notify(IInputSelectNotificationConfig notification)
    {
        return Notify(notification, Guid.NewGuid().ToString());
    }

    public InputSelectNotification Notify(IInputSelectNotificationConfig notification,
        InputSelectNotification notificationToReplace)
    {
        return Notify(notification, notificationToReplace.Id);
    }

    public InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id)
    {
        Publish(new NotifyCommand(id, notification));
        return new InputSelectNotification(id, Disposable.Create(() => RemoveNotification(id)));
    }

    public void RemoveNotification(InputSelectNotification notificationToRemove)
    {
        RemoveNotification(notificationToRemove.Id);
    }

    public void RemoveNotification(string id)
    {
        Publish(new RemoveCommand(id));
    }

    private void Publish(NotificationCommand command)
    {
        lock (_lock)
        {
            if (_pendingUntilFirstSubscriber == null)
            {
                _commands.OnNext(command);
                return;
            }

            _pendingUntilFirstSubscriber.RemoveAll(c => c.Id == command.Id);
            if (command is NotifyCommand)
            {
                _pendingUntilFirstSubscriber.Add(command);
            }
        }
    }
}

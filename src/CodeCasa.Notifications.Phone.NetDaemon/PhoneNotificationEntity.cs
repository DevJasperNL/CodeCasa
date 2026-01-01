using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using CodeCasa.Notifications.Phone.NetDaemon.Config;
using NetDaemon.HassModel;

namespace CodeCasa.Notifications.Phone.NetDaemon;

/// <summary>
/// Class representing a phone notification entity for a specific phone implementation.
/// Provides methods to create, update, and remove notifications.
/// </summary>
public class PhoneNotificationEntity : IDisposable
{
    /// <summary>
    /// Delegate type for sending a notification message to the mobile app.
    /// </summary>
    public delegate void MobileAppNotificationDelegate(string message, string? title = null, object? target = null, object? data = null);

    private readonly MobileAppNotificationDelegate _notify;

    private const string MobileAppNotificationAction = "mobile_app_notification_action";
    private const string ClearNotification = "clear_notification";

    private readonly Lock _lock = new();
    private readonly Dictionary<string, Action[]> _actions = new();
    private readonly IDisposable _eventDisposable;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneNotificationEntity"/> class.
    /// Subscribes to notification action events and sets up the delegate for sending notifications.
    /// </summary>
    public PhoneNotificationEntity(IHaContext haContext, MobileAppNotificationDelegate notifyMethod)
    {
        _notify = notifyMethod;
        _eventDisposable = haContext.Events.Filter<PhoneActionEventData>(MobileAppNotificationAction)
            .Where(e => e.Data != null)
            .Subscribe(e =>
            {
                lock (_lock)
                {
                    if (e.Data == null)
                    {
                        return;
                    }

                    var tag = e.Data.Tag ?? e.Data.ActionData?.Tag;
                    if (tag == null || !_actions.TryGetValue(tag, out var actions))
                    {
                        return;
                    }

                    if (e.Data?.Action == null || !int.TryParse(e.Data?.Action, out int actionId) || actionId < 0 ||
                        actionId >= actions.Length)
                    {
                        return;
                    }

                    actions[actionId].Invoke();
                }
            });
    }

    /// <summary>
    /// Creates and sends a new notification with a generated unique ID.
    /// </summary>
    /// <returns>A <see cref="PhoneNotification"/> handle to the created notification.</returns>
    public PhoneNotification Notify(PhoneNotificationConfig config)
    {
        return Notify(config, Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Updates an existing notification by replacing it with the specified ID.
    /// </summary>
    /// <param name="config">The new notification configuration.</param>
    /// <param name="notificationToReplace">The existing notification to replace.</param>
    /// <returns>A <see cref="PhoneNotification"/> handle to the updated notification.</returns>
    public PhoneNotification Notify(PhoneNotificationConfig config, PhoneNotification notificationToReplace)
    {
        return Notify(config, notificationToReplace.Id);
    }

    /// <summary>
    /// Creates or updates a notification with the specified ID.
    /// </summary>
    /// <param name="config">The notification configuration.</param>
    /// <param name="id">The unique identifier for the notification.</param>
    /// <returns>A <see cref="PhoneNotification"/> handle to the notification.</returns>
    public PhoneNotification Notify(PhoneNotificationConfig config, string id)
    {
        lock (_lock)
        {
            if (config.Actions != null)
            {
                _actions[id] = config.Actions.Select(a => a.Action).ToArray();
            }
            else
            {
                _actions.Remove(id);
            }
        }

        var data = config.ToData(id);
        _notify(config.Message, config.Title, data: data);

        return new PhoneNotification(id, Disposable.Create(() => RemoveNotification(id)));
    }

    /// <summary>
    /// Removes a notification using the given <see cref="PhoneNotification"/> handle.
    /// </summary>
    /// <param name="notificationToRemove">The notification to remove.</param>
    public void RemoveNotification(PhoneNotification notificationToRemove)
    {
        RemoveNotification(notificationToRemove.Id);
    }

    /// <summary>
    /// Removes a notification by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the notification to remove.</param>
    public void RemoveNotification(string id)
    {
        _notify(ClearNotification, data: new { tag = id });
        lock (_lock)
        {
            _actions.Remove(id);
        }
    }

    private record PhoneActionEventData
    {
        [JsonPropertyName("tag")] public string? Tag { get; init; } // Android only
        [JsonPropertyName("action_data")] public iOSActionData? ActionData { get; init; } // iOS only
        [JsonPropertyName("action")] public string? Action { get; init; }
    }

    private record iOSActionData
    {
        [JsonPropertyName("tag")] public string? Tag { get; init; }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _eventDisposable.Dispose();
    }
}
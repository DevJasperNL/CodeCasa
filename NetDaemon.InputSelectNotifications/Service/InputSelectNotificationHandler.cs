using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.InputSelectNotifications.Config;
using NetDaemon.InputSelectNotifications.Interact;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetDaemon.InputSelectNotifications.Helpers;

namespace NetDaemon.InputSelectNotifications.Service;

internal class InputSelectNotificationHandler : IDisposable
{
    private const int MaxMessageLength = 255;
    private const string NotificationClicked = "notification_clicked";

    private readonly IScheduler _scheduler;
    private readonly Lock _lock = new();
    private readonly IEntityCore _inputSelectEntity;
    private readonly IEntityCore? _inputNumberEntity;

    private readonly List<ManagedNotification> _notifications = new();

    private int _nextInternalId;

    public InputSelectNotificationHandler(
        IHaContext haContext,
        IScheduler scheduler, 
        IEntityCore inputSelectEntity,
        IEntityCore? inputNumberEntity,
        InputSelectNotificationEntity inputSelectNotificationEntity)
    {
        _scheduler = scheduler;
        _inputSelectEntity = inputSelectEntity;
        _inputNumberEntity = inputNumberEntity;

        inputSelectNotificationEntity.NotifyObservable.Subscribe(x => Notify(x.id, x.config));
        inputSelectNotificationEntity.RemoveNotificationObservable.Subscribe(RemoveNotification);
        haContext.Events.Filter<NotificationClickedEventData>(NotificationClicked)
            .Where(e =>
                e.Data != null &&
                e.Data.NotificationEntity.Equals(inputSelectEntity.EntityId, StringComparison.OrdinalIgnoreCase) &&
                e.Data.NotificationIndex < _notifications.Count)
            .Subscribe(e =>
            {
                lock (_lock)
                {
                    _notifications[e.Data!.NotificationIndex].Action?.Invoke();
                }
            });

        UpdateOptionsInHomeAssistant();
    }

    private void Notify(string id, InputSelectNotificationConfig notificationConfig)
    {
        if (notificationConfig.Timeout != null && notificationConfig.Timeout < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"Cannot add a notification with a negative timeout ({notificationConfig.Timeout.Value}).");
        }

        lock (_lock)
        {
            var notificationInfo = notificationConfig.ToDashboardNotificationInfo(DateTime.UtcNow);
            var internalId = _nextInternalId++;

            IDisposable? scheduleDisposable = null;
            if (notificationConfig.Timeout != null)
            {
                scheduleDisposable = _scheduler.Schedule(notificationConfig.Timeout.Value, () =>
                {
                    lock (_lock)
                    {
                        _notifications.RemoveAt(_notifications.FindIndex(n => n.InternalId == internalId));

                        UpdateOptionsInHomeAssistant();
                    }
                });
            }

            var managedNotificationToInsert = new ManagedNotification(
                id, internalId, notificationConfig.Action,
                notificationConfig.Order, notificationInfo, scheduleDisposable);

            var index = _notifications.FindIndex(n => n.Id == id);
            if (index == -1)
            {
                var insertIndex = _notifications.FindIndex(n => n.Order.GetValueOrDefault(0) >= notificationConfig.Order.GetValueOrDefault(0));
                if (insertIndex == -1)
                {
                    insertIndex = _notifications.Count;
                }

                _notifications.Insert(insertIndex, managedNotificationToInsert);
            }
            else
            {
                _notifications[index].ScheduleDisposable?.Dispose();

                _notifications[index] = managedNotificationToInsert;
            }

            UpdateOptionsInHomeAssistant();
        }
    }

    private void RemoveNotification(string id)
    {
        lock (_lock)
        {
            var index = _notifications.FindIndex(n => n.Id == id);
            if (index == -1)
            {
                return;
            }
            _notifications[index].ScheduleDisposable?.Dispose();
            _notifications.RemoveAt(index);

            UpdateOptionsInHomeAssistant();
        }
    }

    private void UpdateOptionsInHomeAssistant()
    {
        if (!_notifications.Any())
        {
            _inputSelectEntity.CallService("set_options", new { options = new[] { string.Empty } });
            _inputNumberEntity?.CallService("set_value", new { value = 0 });
            return;
        }

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        var options = _notifications.Select(n =>
        {
            var dashboardNotificationInfo = n.DashboardNotificationInfo;
            var json = JsonSerializer.Serialize(dashboardNotificationInfo, jsonSerializerOptions);
            if (json.Length <= MaxMessageLength)
            {
                return json;
            }
            var (message, secondaryMessage) = MessageShortener.ShortenMessages(
                dashboardNotificationInfo.Message,
                dashboardNotificationInfo.SecondaryMessage,
                json.Length - MaxMessageLength);
            return JsonSerializer.Serialize(dashboardNotificationInfo with { Message = message, SecondaryMessage = secondaryMessage }, jsonSerializerOptions);
        }).ToArray();

        _inputSelectEntity.CallService("set_options", new { options });
        _inputNumberEntity?.CallService("set_value", new { value = _notifications.Count });
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record NotificationClickedEventData
    {
        [JsonPropertyName("notificationEntity")]
        public string NotificationEntity { get; init; } = null!;

        [JsonPropertyName("notificationIndex")]
        public int NotificationIndex { get; init; }
    }

    private record ManagedNotification(
        string? Id,
        long InternalId,
        Action? Action,
        int? Order,
        DashboardNotificationInfo DashboardNotificationInfo,
        IDisposable? ScheduleDisposable);
}
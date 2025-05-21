using System.Reactive.Linq;
using NetDaemon.HassModel;
using System.Text.Json.Serialization;
using System.Reactive.Disposables;
using NetDaemon.PhoneNotifications.Config;

namespace NetDaemon.PhoneNotifications
{
    public abstract class AndroidPhoneNotificationEntity(IHaContext haContext)
        : PhoneNotificationEntity<AndroidNotificationConfig, AndroidNotificationAction>(haContext);

    public abstract class IosPhoneNotificationEntity(IHaContext haContext)
        : PhoneNotificationEntity<IosNotificationConfig, IosNotificationAction>(haContext);

    public abstract class PhoneNotificationEntity<TPhoneNotificationConfig, TPhoneNotificationAction>
        where TPhoneNotificationConfig : PhoneNotificationConfig<TPhoneNotificationAction>
        where TPhoneNotificationAction : NotificationAction
    {
        private const string MobileAppNotificationAction = "mobile_app_notification_action";
        private const string ClearNotification = "clear_notification";

        private readonly Lock _lock = new();
        private readonly Dictionary<string, Action[]> _actions = new();

        protected PhoneNotificationEntity(IHaContext haContext)
        {
            haContext.Events.Filter<PhoneActionEventData>(MobileAppNotificationAction)
                .Where(e => e.Data != null)
                .Subscribe(e =>
                {
                    lock (_lock)
                    {
                        if (e.Data?.Tag == null || !_actions.TryGetValue(e.Data.Tag, out var actions))
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

        public PhoneNotification Notify(TPhoneNotificationConfig config)
        {
            return Notify(config, Guid.NewGuid().ToString());
        }

        public PhoneNotification Notify(TPhoneNotificationConfig config, PhoneNotification notificationToReplace)
        {
            return Notify(config, notificationToReplace.Id);
        }

        public PhoneNotification Notify(TPhoneNotificationConfig config, string id)
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
            NotificationServiceNotifyImplementation(config.Message, config.Title, data);

            return new PhoneNotification(id, Disposable.Create(() => RemoveNotification(id)));
        }

        public void RemoveNotification(PhoneNotification notificationToRemove)
        {
            RemoveNotification(notificationToRemove.Id);
        }

        public void RemoveNotification(string id)
        {
            NotificationServiceNotifyImplementation(ClearNotification, null, new { tag = id });
            lock (_lock)
            {
                _actions.Remove(id);
            }
        }

        protected abstract void NotificationServiceNotifyImplementation(string message, string? title, object? data);

        private record PhoneActionEventData
        {
            [JsonPropertyName("tag")] public string? Tag { get; init; }
            [JsonPropertyName("action")] public string? Action { get; init; }
        }
    }
}

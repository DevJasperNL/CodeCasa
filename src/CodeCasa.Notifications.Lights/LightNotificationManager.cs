using System.Reactive.Disposables;
using System.Reactive.Subjects;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights
{
    /// <summary>
    /// Manages light notifications, handling their priorities and active states.
    /// </summary>
    public class LightNotificationManager
    {
        private readonly Lock _lock = new();
        private readonly BehaviorSubject<ILightNotificationConfig?> _subject = new(null);

        private readonly Dictionary<string, ILightNotificationConfig> _activeNotifications = new();

        /// <summary>
        /// Gets an observable sequence of the current active light notification configuration.
        /// </summary>
        /// <returns>An observable that emits the current light notification configuration or null if none are active.</returns>
        public IObservable<ILightNotificationConfig?> LightNotifications()
        {
            lock (_lock)
            {
                return _subject;
            }
        }

        /// <summary>
        /// Notifies with a new light notification configuration, replacing an existing notification.
        /// </summary>
        /// <param name="lightNotificationConfig">The configuration for the new notification.</param>
        /// <param name="lightNotificationToReplace">The existing notification to replace.</param>
        /// <returns>The created light notification.</returns>
        public LightNotification Notify(ILightNotificationConfig lightNotificationConfig, LightNotification lightNotificationToReplace)
        {
            return Notify(lightNotificationConfig, lightNotificationToReplace.Id);
        }

        /// <summary>
        /// Notifies with a new light notification configuration using a specific ID.
        /// </summary>
        /// <param name="lightNotificationConfig">The configuration for the new notification.</param>
        /// <param name="id">The unique identifier for the notification.</param>
        /// <returns>The created light notification.</returns>
        public LightNotification Notify(ILightNotificationConfig lightNotificationConfig, string id)
        {
            lock (_lock)
            {
                var highestPrio = _activeNotifications.Any() ? (int?)_activeNotifications.Values.Max(n => n.Priority) : null;
                if (highestPrio == null || lightNotificationConfig.Priority >= highestPrio)
                {
                    _subject.OnNext(lightNotificationConfig);
                }

                _activeNotifications[id] = lightNotificationConfig;
                return new LightNotification(id, Disposable.Create(() => Remove(id)));
            }
        }

        /// <summary>
        /// Removes a light notification by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the notification to remove.</param>
        /// <returns>True if the notification was successfully removed; otherwise, false.</returns>
        public bool Remove(string id)
        {
            lock (_lock)
            {
                if (!_activeNotifications.Remove(id, out var configAndDisposable))
                {
                    return false;
                }

                if (!_activeNotifications.Any())
                {
                    _subject.OnNext(null);
                    return true;
                }

                var highestKvp = _activeNotifications.MaxBy(kvp => kvp.Value.Priority);
                if (configAndDisposable.Priority < highestKvp.Value.Priority)
                {
                    return true;
                }

                _subject.OnNext(highestKvp.Value);
                return true;
            }
        }
    }
}

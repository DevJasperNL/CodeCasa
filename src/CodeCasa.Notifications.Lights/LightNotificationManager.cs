using System.Reactive.Disposables;
using System.Reactive.Subjects;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights
{
    public class LightNotificationManager
    {
        private readonly Lock _lock = new();
        private readonly BehaviorSubject<LightNotificationConfig?> _subject = new(null);

        private readonly Dictionary<string, LightNotificationConfig> _activeNotifications = new();

        public IObservable<LightNotificationConfig?> LightNotifications()
        {
            lock (_lock)
            {
                return _subject;
            }
        }

        public LightNotification Notify(LightNotificationConfig lightNotificationOptions, LightNotification lightNotificationToReplace)
        {
            return Notify(lightNotificationOptions, lightNotificationToReplace.Id);
        }

        public LightNotification Notify(LightNotificationConfig lightNotificationOptions, string id)
        {
            lock (_lock)
            {
                var highestPrio = _activeNotifications.Any() ? (int?)_activeNotifications.Values.Max(n => n.Priority) : null;
                if (highestPrio == null || lightNotificationOptions.Priority >= highestPrio)
                {
                    _subject.OnNext(lightNotificationOptions);
                }

                _activeNotifications[id] = lightNotificationOptions;
                return new LightNotification(id, Disposable.Create(() => Remove(id)));
            }
        }

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

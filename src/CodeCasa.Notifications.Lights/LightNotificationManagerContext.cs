using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights
{
    public class LightNotificationManagerContext : IDisposable
    {
        private readonly BehaviorSubject<LightNotificationConfig?> _subject = new(null);
        private readonly IDisposable _subscriptionDisposable;

        public LightNotificationManagerContext(LightNotificationManager lightNotificationManager)
        {
            _subscriptionDisposable = lightNotificationManager.LightNotifications().Subscribe(_subject);
        }

        public IObservable<LightNotificationConfig?> LightNotifications => _subject.AsObservable();

        public void Dispose()
        {
            _subscriptionDisposable.Dispose();
        }
    }
}

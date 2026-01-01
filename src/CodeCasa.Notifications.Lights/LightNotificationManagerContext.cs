using System.Reactive.Linq;
using System.Reactive.Subjects;
using CodeCasa.Notifications.Lights.Config;

namespace CodeCasa.Notifications.Lights
{
    /// <summary>
    /// Represents a context that subscribes to light notifications from a manager and exposes them as an observable.
    /// </summary>
    public class LightNotificationManagerContext : IDisposable
    {
        private readonly BehaviorSubject<LightNotificationConfig?> _subject = new(null);
        private readonly IDisposable _subscriptionDisposable;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightNotificationManagerContext"/> class.
        /// </summary>
        /// <param name="lightNotificationManager">The manager to subscribe to.</param>
        public LightNotificationManagerContext(LightNotificationManager lightNotificationManager)
        {
            _subscriptionDisposable = lightNotificationManager.LightNotifications().Subscribe(_subject);
        }

        /// <summary>
        /// Gets an observable sequence of the current light notification configuration.
        /// </summary>
        public IObservable<LightNotificationConfig?> LightNotifications => _subject.AsObservable();

        /// <inheritdoc />
        public void Dispose()
        {
            _subscriptionDisposable.Dispose();
        }
    }
}

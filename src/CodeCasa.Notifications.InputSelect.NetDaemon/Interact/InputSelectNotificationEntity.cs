
using CodeCasa.NetDaemon.Notifications.InputSelect.Config;

namespace CodeCasa.NetDaemon.Notifications.InputSelect.Interact
{
    /// <summary>
    /// Abstract base class that acts as a convenient wrapper around <see cref="IInputSelectNotificationEntity"/>.
    /// Intended to simplify creation of custom input select notification entities by delegating calls to an inner implementation.
    /// </summary>
    /// <param name="inner">The inner <see cref="IInputSelectNotificationEntity"/> instance to delegate to.</param>
    public abstract class InputSelectNotificationEntity(IInputSelectNotificationEntity inner)
        : IInputSelectNotificationEntity
    {
        private readonly IInputSelectNotificationEntity _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        /// <inheritdoc />
        public string InputSelectEntityId => _inner.InputSelectEntityId;

        /// <inheritdoc />
        public string? InputNumberEntityId => _inner.InputNumberEntityId;

        /// <inheritdoc />
        public virtual InputSelectNotification Notify(IInputSelectNotificationConfig notification)
        {
            return _inner.Notify(notification);
        }

        /// <inheritdoc />
        public virtual InputSelectNotification Notify(IInputSelectNotificationConfig notification, InputSelectNotification notificationToReplace)
        {
            return _inner.Notify(notification, notificationToReplace);
        }

        /// <inheritdoc />
        public virtual InputSelectNotification Notify(IInputSelectNotificationConfig notification, string id)
        {
            return _inner.Notify(notification, id);
        }

        /// <inheritdoc />
        public virtual void RemoveNotification(InputSelectNotification notificationToRemove)
        {
            _inner.RemoveNotification(notificationToRemove);
        }

        /// <inheritdoc />
        public virtual void RemoveNotification(string id)
        {
            _inner.RemoveNotification(id);
        }
    }
}

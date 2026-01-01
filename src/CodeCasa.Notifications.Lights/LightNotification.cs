namespace CodeCasa.Notifications.Lights
{
    public class LightNotification(string id, IDisposable disposable) : IDisposable
    {
        /// <summary>
        /// Gets the unique identifier for the notification.
        /// This ID can be used to update or cancel the notification later.
        /// </summary>
        public string Id { get; } = id;

        /// <inheritdoc />
        public void Dispose() => disposable.Dispose();
    }
}

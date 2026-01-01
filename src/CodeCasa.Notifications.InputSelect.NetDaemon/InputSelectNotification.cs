namespace CodeCasa.NetDaemon.Notifications.InputSelect;

/// <summary>
/// Represents a handle to an input select notification that has been created.
/// This object can be used to manage the lifecycle of the notification,
/// such as disposing it to remove or cancel the notification, or updating its content.
/// </summary>
/// <param name="id">A unique identifier for the notification.</param>
/// <param name="disposable">An <see cref="IDisposable"/> resource associated with the notification's lifetime.</param>
public class InputSelectNotification(string id, IDisposable disposable) : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for the notification.
    /// This ID can be used to update or cancel the notification later.
    /// </summary>
    public string Id { get; } = id;

    /// <inheritdoc />
    public void Dispose() => disposable.Dispose();
}
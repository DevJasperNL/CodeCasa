namespace CodeCasa.NetDaemon.Notifications.InputSelect.Config;

/// <summary>
/// Defines configuration options for an input select notification list.
/// </summary>
public interface IInputSelectNotificationConfig
{
    /// <summary>
    /// Gets or sets the optional timeout duration after which the notification should automatically dismiss.
    /// </summary>
    TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets the optional action to execute when the notification is interacted with.
    /// </summary>
    Action? Action { get; set; }

    /// <summary>
    /// Gets or sets the optional order priority of the notification.
    /// Lower values indicate higher in the list.
    /// </summary>
    int? Order { get; set; }

    /// <summary>
    /// Converts the configuration to a string representation suitable for input select options.
    /// </summary>
    string ToInputSelectOptionString();
}
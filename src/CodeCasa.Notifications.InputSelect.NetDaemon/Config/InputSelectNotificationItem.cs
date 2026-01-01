namespace CodeCasa.NetDaemon.Notifications.InputSelect.Config;

/// <summary>
/// Represents a configuration item for an input select notification.
/// </summary>
public class InputSelectNotificationItem
{
    /// <summary>
    /// Gets or sets the entity ID of the input select entity to manage a notification list for.
    /// This property is required.
    /// </summary>
    public string InputSelectEntityId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the optional entity ID of an input number entity associated with this notification configuration.
    /// If provided, it will be updated with the amount of notifications present in <see cref="InputSelectEntityId"/>.
    /// </summary>
    public string? InputNumberEntityId { get; set; }
}
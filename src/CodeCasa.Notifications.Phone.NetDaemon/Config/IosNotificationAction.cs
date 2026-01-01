namespace CodeCasa.NetDaemon.Notifications.Phone.Config;

/// <summary>
/// Represents an iOS-specific notification action, extending the base notification action with iOS-only properties.
/// </summary>
public record IosNotificationAction(Action Action, string Title) : NotificationAction(Action, Title)
{
    /// <summary>
    /// Optional icon to display alongside the action button in the notification.
    /// </summary>
    public string? Icon { get; set; }

    // Note: additional options available.

    /// <inheritdoc />
    public override object ToData(int index)
    {
        return new
        {
            action = $"{index}",
            title = Title,
            uri = Uri,
            icon = Icon
        };
    }
}
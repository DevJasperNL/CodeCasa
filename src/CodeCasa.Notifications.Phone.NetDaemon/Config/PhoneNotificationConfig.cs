namespace CodeCasa.NetDaemon.Notifications.Phone.Config;

/// <summary>
/// Abstract base class for phone notification config implementations
/// </summary>
public abstract record PhoneNotificationConfig(string Title, string Message)
{
    /// <inheritdoc />
    protected PhoneNotificationConfig(string title) : this(title, string.Empty)
    {
    }

    // Can be used for record copying
    /// <inheritdoc />
    protected PhoneNotificationConfig() : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    /// The title of the notification.
    /// </summary>
    public string? Title { get; set; } = Title;

    /// <summary>
    /// The main message or body text of the notification.
    /// </summary>
    public string Message { get; set; } = Message;

    /// <summary>
    /// Optional group identifier used for visually grouping notifications together.
    /// </summary>
    public string? Group { get; set; } // Used for grouping notifications together visually

    /// <summary>
    /// Optional actions the user can perform directly from the notification.
    /// </summary>
    public NotificationAction[]? Actions { get; set; }

    /// <summary>
    /// Optional URL or resource identifier of an image to display with the notification.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional URL or resource identifier of a video to display with the notification.
    /// </summary>
    public string? Video { get; set; }

    // Note: Dynamic map is possible.

    // Note: Dynamic actions can be used to show a map or camera stream.

    /// <summary>
    /// Converts the notification configuration to a data object, using the provided tag.
    /// </summary>
    public abstract object ToData(string tag);
}
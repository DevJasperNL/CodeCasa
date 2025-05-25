namespace NetDaemon.PhoneNotifications.Config;

public abstract record PhoneNotificationConfig(string Title, string Message)
{
    protected PhoneNotificationConfig(string title) : this(title, string.Empty)
    {
    }

    // Can be used for templating
    protected PhoneNotificationConfig() : this(string.Empty, string.Empty)
    {
    }

    public string? Title { get; set; } = Title;
    public string Message { get; set; } = Message;
    public string? Group { get; set; } // Used for grouping notifications together visually

    public NotificationAction[]? Actions { get; set; }

    public string? Image { get; set; }
    public string? Video { get; set; }
    // Note: Dynamic map is possible.

    // Note: Dynamic actions can be used to show a map or camera stream.

    public abstract object ToData(string tag);
}
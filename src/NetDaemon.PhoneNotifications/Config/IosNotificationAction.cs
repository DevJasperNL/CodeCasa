namespace NetDaemon.PhoneNotifications.Config;

public record IosNotificationAction(Action Action, string Title) : NotificationAction(Action, Title)
{
    public string? Icon { get; set; }

    // todo: extend with additional options

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
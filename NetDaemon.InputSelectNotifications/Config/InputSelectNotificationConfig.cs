using System.Drawing;

namespace NetDaemon.InputSelectNotifications.Config;

public record InputSelectNotificationConfig(string Message)
{
    // Can be used for templating
    public InputSelectNotificationConfig() : this(string.Empty)
    {
    }

    public string Message { get; set; } = Message;
    public string? SecondaryMessage { get; set; }
    public string? Icon { get; set; }
    public Color? IconColor { get; set; }
    public string? BadgeIcon { get; set; }
    public Color? BadgeIconColor { get; set; }
    public TimeSpan? Timeout { get; set; }
    public Action? Action { get; set; }
    public int? Order { get; set; }

    internal DashboardNotificationInfo ToDashboardNotificationInfo(DateTime utcTimeStamp)
    {
        return new DashboardNotificationInfo(Message)
        {
            SecondaryMessage = SecondaryMessage,
            Icon = Icon,
            IconColor = IconColor?.Name.ToLowerInvariant(),
            BadgeIcon = BadgeIcon,
            BadgeColor = BadgeIconColor?.Name.ToLowerInvariant(),
            TimeStamp = utcTimeStamp.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}
using System.Drawing;

namespace NetDaemon.InputSelectNotifications.Interact
{
    public record InputSelectNotificationConfig(string Message)
    {
        // Can be used for templating
        public InputSelectNotificationConfig() : this(string.Empty)
        {
        }

        public string Message { get; set; } = Message;
        public string? SecondaryMessage { get; set; }
        public string? Icon { get; set; }
        public Color? Color { get; set; }
        public string? BadgeIcon { get; set; }
        public Color? BadgeColor { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Action? Action { get; set; }
        public int? Order { get; set; }

        public DashboardNotificationInfo ToDashboardNotificationInfo(DateTime utcTimeStamp)
        {
            return new DashboardNotificationInfo(Message)
            {
                SecondaryMessage = SecondaryMessage,
                Icon = Icon,
                Color = Color?.Name.ToLowerInvariant(),
                BadgeIcon = BadgeIcon,
                BadgeColor = BadgeColor?.Name.ToLowerInvariant(),
                TimeStamp = utcTimeStamp.Subtract(TimeSpan.FromSeconds(5)).ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }
    }
}

using CodeCasa.CustomEntities.Core.Notifications;

namespace CodeCasa.Dashboard.ViewModels
{
    public class NotificationVm
    {
        public NotificationVm(LivingRoomPanelNotification notification)
        {
            Notification = notification;
            TimeAgoMessage = DetermineTimeAgoMessage();
        }

        public LivingRoomPanelNotification Notification { get; }
        public string? TimeAgoMessage { get; private set; }

        public bool Update()
        {
            var newMessage = DetermineTimeAgoMessage();
            if (TimeAgoMessage != newMessage)
            {
                TimeAgoMessage = newMessage;
                return true;
            }

            return false;
        }

        private string? DetermineTimeAgoMessage() => Notification.TimeStamp == null
            ? null
            : FormatTimeAgo(DateTimeOffset.Parse(Notification.TimeStamp));

        private static string FormatTimeAgo(DateTimeOffset notificationMoment)
        {
            var timeSpan = DateTimeOffset.UtcNow - notificationMoment;

            if (timeSpan.TotalSeconds < 60)
                return "just now";

            if (timeSpan.TotalMinutes < 2)
                return "1 min";

            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min";

            if (timeSpan.TotalHours < 2)
                return "1 hour";

            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours";

            if (timeSpan.TotalDays < 2)
                return "yesterday";

            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days";

            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks";

            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months";

            return $"{(int)(timeSpan.TotalDays / 365)} years";
        }
    }
}

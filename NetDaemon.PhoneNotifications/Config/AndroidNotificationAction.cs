namespace NetDaemon.PhoneNotifications.Config
{
    public record AndroidNotificationAction(Action Action, string Title) : NotificationAction(Action, Title)
    {
        public override object ToData(int index)
        {
            return new
            {
                action = $"{index}",
                title = Title,
                uri = Uri
            };
        }
    }
}

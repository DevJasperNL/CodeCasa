namespace NetDaemon.PhoneNotifications.Config
{
    public record NotificationAction(Action Action, string Title)
    {
        public string? Uri { get; set; }

        public virtual object ToData(int index)
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

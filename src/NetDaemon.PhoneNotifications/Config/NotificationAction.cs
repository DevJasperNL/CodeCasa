namespace NetDaemon.PhoneNotifications.Config
{
    public abstract record NotificationAction(Action Action, string Title)
    {
        public string? Uri { get; set; }

        public abstract object ToData(int index);
    }
}

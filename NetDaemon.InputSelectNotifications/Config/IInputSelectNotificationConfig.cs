namespace NetDaemon.InputSelectNotifications.Config;

public interface IInputSelectNotificationConfig
{
    public TimeSpan? Timeout { get; set; }
    public Action? Action { get; set; }
    public int? Order { get; set; }

    public string ToInputSelectOptionString();
}
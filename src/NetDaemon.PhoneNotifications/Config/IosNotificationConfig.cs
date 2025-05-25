namespace NetDaemon.PhoneNotifications.Config;

public record IosNotificationConfig : PhoneNotificationConfig
{
    public string? Url { get; set; }
    public string? Subtitle { get; set; }
    public string? Audio { get; set; }
    public bool? HideThumbnail { get; set; }

    // Note: content-type can be used to manually specify URL content type. This will usually automatically be inferred from the extension.
    // Note: camera stream is possible.

    public override object ToData(string tag)
    {
        return new
        {
            subtitle = Subtitle,
            url = Url,
            group = Group,
            tag,
            actions = Actions?.Select((a, index) => a.ToData(index)).ToArray(),
            image = Image,
            video = Video,
            audio = Audio,
            hide_thumbnail = HideThumbnail
        };
    }
}
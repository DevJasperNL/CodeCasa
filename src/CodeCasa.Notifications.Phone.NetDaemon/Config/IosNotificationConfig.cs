namespace CodeCasa.NetDaemon.Notifications.Phone.Config;

/// <summary>
/// Configuration options specific to iOS notifications.
/// </summary>
public record IosNotificationConfig : PhoneNotificationConfig
{
    /// <summary>
    /// Optional URL to open when the notification is tapped.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Optional subtitle text displayed below the title in the notification.
    /// </summary>
    public string? Subtitle { get; set; }

    /// <summary>
    /// Optional audio file name or URL to play when the notification is delivered.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// If true, hides the thumbnail image in the notification.
    /// </summary>
    public bool? HideThumbnail { get; set; }

    // Note: content-type can be used to manually specify URL content type. This will usually automatically be inferred from the extension.
    // Note: camera stream is possible.

    /// <inheritdoc />
    public override object ToData(string tag)
    {
        return new
        {
            subtitle = Subtitle,
            url = Url,
            group = Group,
            tag,
            actions = Actions?.Select((a, index) => a.ToData(index)).ToArray(),
            action_data = new // For iOS, data in this object is returned in the mobile_app_notification_action event.
            {
                tag
            },
            image = Image,
            video = Video,
            audio = Audio,
            hide_thumbnail = HideThumbnail
        };
    }
}
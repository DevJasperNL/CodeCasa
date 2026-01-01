using System.Drawing;
using System.Globalization;
using CodeCasa.Notifications.Phone.NetDaemon.Extensions;

namespace CodeCasa.Notifications.Phone.NetDaemon.Config;

/// <summary>
/// Configuration options specific to Android notifications.
/// </summary>
public record AndroidNotificationConfig : PhoneNotificationConfig
{
    /// <summary>
    /// Used for opening a URL when clicking the notification.
    /// </summary>
    public string? ClickAction { get; set; }

    /// <summary>
    /// The subject text of the notification.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// The accent color to apply to the notification.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Whether the notification should be sticky.
    /// </summary>
    public bool Sticky { get; set; }

    /// <summary>
    /// The channel name for grouping notifications.
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// The importance level of the notification channel.
    /// </summary>
    public ChannelImportanceValues? ChannelImportance { get; set; }

    /// <summary>
    /// Custom vibration pattern for the notification channel.
    /// </summary>
    public int[]? ChannelVibrationPattern { get; set; }

    /// <summary>
    /// Whether the notification should be persistent.
    /// </summary>
    public bool Persistent { get; set; }

    /// <summary>
    /// LED color for the notification channel.
    /// </summary>
    public Color? ChannelLedColor { get; set; }

    /// <summary>
    /// Duration after which the notification should be automatically removed.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// URL of the icon to display in the notification.
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Visibility level of the notification on the lock screen.
    /// </summary>
    public Visibilities Visibility { get; set; } = Visibilities.Private;

    // Note: TTS is available for android. Not implemented due to lack of a use case yet.

    /// <summary>
    /// An optional counter value associated with the notification.
    /// </summary>
    public DateTime? Counter;

    /// <summary>
    /// If true, the notification alerts only once.
    /// </summary>
    public bool AlertOnce;

    /// <summary>
    /// Name of the status bar icon to use.
    /// </summary>
    public string? StatusBarIcon { get; set; }

    /// <summary>
    /// Enables Android Auto support for the notification.
    /// </summary>
    public bool AndroidAuto { get; set; }

    /// <inheritdoc />
    public override object ToData(string tag)
    {
        return new
        {
            subject = Subject,
            clickAction = ClickAction,
            group = Group,
            tag,
            color = Color?.ToHex(),
            sticky = Sticky,
            channel = Channel,
            importance = ChannelImportance.ToString()?.ToLowerInvariant(),
            vibrationPattern = ChannelVibrationPattern == null ? null : string.Join(", ", ChannelVibrationPattern),
            ledColor = ChannelLedColor?.ToHex(),
            persistent = Persistent,
            timeout =
                Timeout == null ? null : ((int)Math.Max(1, Math.Round(Timeout.Value.TotalSeconds))).ToString(),
            icon_url = IconUrl,
            visibility = Visibility.ToString().ToLowerInvariant(),
            chronometer = Counter != null,
            when = Counter?.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
            alert_once = AlertOnce,
            notification_icon = StatusBarIcon,
            car_ui = AndroidAuto,
            actions = Actions?.Select((a, index) => a.ToData(index)).ToArray(),
            image = Image,
            video = Video
        };
    }

    /// <summary>
    /// Specifies the importance level of an Android notification channel.
    /// Controls how notifications behave and how prominently they appear to the user.
    /// </summary>
    public enum ChannelImportanceValues
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        High,
        Low,
        Max,
        Min,
        Default
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Defines how much of a notification’s content is visible on the device lock screen.
    /// </summary>
    public enum Visibilities
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        Public,
        Private,
        Secret
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
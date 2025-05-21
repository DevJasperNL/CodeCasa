using System.Drawing;
using System.Globalization;
using NetDaemon.PhoneNotifications.Extensions;

namespace NetDaemon.PhoneNotifications.Config
{
    public record AndroidNotificationConfig : PhoneNotificationConfig<AndroidNotificationAction>
    {
        public string? ClickAction { get; set; } // Used for opening an URL when clicking the notification
        public string? Subject { get; set; }
        public Color? Color { get; set; }
        public bool Sticky { get; set; }
        public string? Channel { get; set; }
        public ChannelImportanceValues? ChannelImportance { get; set; }
        public int[]? ChannelVibrationPattern { get; set; }
        public bool Persistent { get; set; }
        public Color? ChannelLedColor { get; set; }
        public TimeSpan? Timeout { get; set; }
        public string? IconUrl { get; set; }
        public Visibilities Visibility { get; set; } = Visibilities.Private;
        // Note: TTS is available for android. Not implemented due to lack of a use case yet.
        public DateTime? Counter;
        public bool AlertOnce;
        public string? StatusBarIcon { get; set; }
        public bool AndroidAuto { get; set; }

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

        public enum ChannelImportanceValues
        {
            High,
            Low,
            Max,
            Min,
            Default
        }

        public enum Visibilities
        {
            Public,
            Private,
            Secret
        }
    }
}

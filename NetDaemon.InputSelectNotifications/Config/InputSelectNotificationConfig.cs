using NetDaemon.InputSelectNotifications.Helpers;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetDaemon.InputSelectNotifications.Config;

public record InputSelectNotificationConfig(string Message) : IInputSelectNotificationConfig
{
    private const int MaxMessageLength = 255;

    // Can be used for record copying
    public InputSelectNotificationConfig() : this(string.Empty)
    {
    }

    public string Message { get; set; } = Message;
    public string? SecondaryMessage { get; set; }
    public string? Icon { get; set; }
    public Color? IconColor { get; set; }
    public string? BadgeIcon { get; set; }
    public Color? BadgeIconColor { get; set; }
    public string? BadgeContent { get; set; }
    public TimeSpan? Timeout { get; set; }
    public Action? Action { get; set; }
    public int? Order { get; set; }

    public string ToInputSelectOptionString()
    {
        var notificationInfo = new DashboardNotificationInfo(Message)
        {
            SecondaryMessage = SecondaryMessage,
            Icon = Icon,
            IconColor = IconColor?.Name.ToLowerInvariant(),
            BadgeIcon = BadgeIcon,
            BadgeColor = BadgeIconColor?.Name.ToLowerInvariant(),
            BadgeContent = BadgeContent,
            TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        var json = JsonSerializer.Serialize(notificationInfo, jsonSerializerOptions);
        if (json.Length <= MaxMessageLength)
        {
            return json;
        }
        var (message, secondaryMessage) = MessageShortener.ShortenMessages(
            notificationInfo.Message,
            notificationInfo.SecondaryMessage,
            json.Length - MaxMessageLength);
        return JsonSerializer.Serialize(notificationInfo with { Message = message, SecondaryMessage = secondaryMessage }, jsonSerializerOptions);
    }
}
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeCasa.NetDaemon.Notifications.InputSelect.Helpers;

namespace CodeCasa.NetDaemon.Notifications.InputSelect.Config;

/// <summary>
/// Default implementation of <see cref="IInputSelectNotificationConfig"/> provided by the library.
/// </summary>
/// <param name="Message">The main message to display in the notification.</param>
public record InputSelectDashboardNotificationConfig(string Message) : IInputSelectNotificationConfig
{
    private const int MaxMessageLength = 255;

    /// <summary>
    /// Initializes a new instance of the <see cref="InputSelectDashboardNotificationConfig"/> record with an empty message.
    /// Useful for record copying or default construction.
    /// </summary>
    public InputSelectDashboardNotificationConfig() : this(string.Empty)
    {
    }

    /// <summary>
    /// Gets or sets the main message displayed in the notification.
    /// </summary>
    public string Message { get; set; } = Message;

    /// <summary>
    /// Gets or sets an optional secondary message to display alongside the main message.
    /// </summary>
    public string? SecondaryMessage { get; set; }

    /// <summary>
    /// Gets or sets the optional icon name or URI to display with the notification.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the optional color to apply to the icon.
    /// </summary>
    public Color? IconColor { get; set; }

    /// <summary>
    /// Gets or sets the optional badge icon name or URI.
    /// </summary>
    public string? BadgeIcon { get; set; }

    /// <summary>
    /// Gets or sets the optional color to apply to the badge icon.
    /// </summary>
    public Color? BadgeIconColor { get; set; }

    /// <summary>
    /// Gets or sets optional content text to display inside the badge.
    /// </summary>
    public string? BadgeContent { get; set; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; set; }

    /// <inheritdoc/>
    public Action? Action { get; set; }

    /// <inheritdoc/>
    public int? Order { get; set; }

    /// <inheritdoc />
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
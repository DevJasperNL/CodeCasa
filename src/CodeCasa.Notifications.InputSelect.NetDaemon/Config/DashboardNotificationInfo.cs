using System.Text.Json.Serialization;

namespace CodeCasa.Notifications.InputSelect.NetDaemon.Config;

/// <summary>
/// Represents a dashboard notification information structure to be serialized as JSON.
/// This structure is optimized to keep property names short because the entire JSON string
/// must fit within a 256-character limit, such as when used as a string value in an input select option.
/// </summary>
public record DashboardNotificationInfo(string Message)
{
    /// <summary>
    /// Gets or sets the main notification message.
    /// Serialized as 'm' in JSON.
    /// </summary>
    [JsonPropertyName("m")]
    public string Message { get; set; } = Message;

    /// <summary>
    /// Gets or sets an optional secondary message for additional information.
    /// Serialized as 's' in JSON.
    /// </summary>
    [JsonPropertyName("s")]
    public string? SecondaryMessage { get; set; }

    /// <summary>
    /// Gets or initializes an optional icon name to be displayed with the notification.
    /// Serialized as 'i' in JSON.
    /// </summary>
    [JsonPropertyName("i")]
    public string? Icon { get; init; }

    /// <summary>
    /// Gets or initializes an optional color for the icon.
    /// Serialized as 'c' in JSON.
    /// </summary>
    [JsonPropertyName("c")]
    public string? IconColor { get; init; }

    /// <summary>
    /// Gets or initializes an optional badge icon name to be displayed alongside the notification.
    /// Serialized as 'b' in JSON.
    /// </summary>
    [JsonPropertyName("b")]
    public string? BadgeIcon { get; init; }

    /// <summary>
    /// Gets or initializes optional badge content text.
    /// Serialized as 'n' in JSON.
    /// </summary>
    [JsonPropertyName("n")]
    public string? BadgeContent { get; init; }

    /// <summary>
    /// Gets or initializes an optional color for the badge.
    /// Serialized as 'o' in JSON.
    /// </summary>
    [JsonPropertyName("o")]
    public string? BadgeColor { get; init; }

    /// <summary>
    /// Gets or initializes an optional timestamp string associated with the notification.
    /// Serialized as 't' in JSON.
    /// </summary>
    [JsonPropertyName("t")]
    public string? TimeStamp { get; init; }
}
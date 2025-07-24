using System.Text.Json.Serialization;

namespace NetDaemon.InputSelectNotifications.Config;

public record DashboardNotificationInfo(string Message)
{
    // Note: we keep property names as short as possible as the limit of the whole json is 256 chars.
    [JsonPropertyName("m")]
    public string Message { get; set; } = Message;
    [JsonPropertyName("s")]
    public string? SecondaryMessage { get; set; }
    [JsonPropertyName("i")]
    public string? Icon { get; init; }
    [JsonPropertyName("c")]
    public string? IconColor { get; init; }
    [JsonPropertyName("b")]
    public string? BadgeIcon { get; init; }
    [JsonPropertyName("n")]
    public string? BadgeContent { get; init; }
    [JsonPropertyName("o")]
    public string? BadgeColor { get; init; }
    [JsonPropertyName("t")]
    public string? TimeStamp { get; init; }
}
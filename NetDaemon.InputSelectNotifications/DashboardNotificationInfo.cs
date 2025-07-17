using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NetDaemon.InputSelectNotifications
{
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
        public string? Color { get; init; }
        [JsonPropertyName("b")]
        public string? BadgeIcon { get; init; }
        [JsonPropertyName("bc")]
        public string? BadgeColor { get; init; }
        [JsonPropertyName("t")]
        public string? TimeStamp { get; init; }
    }
}

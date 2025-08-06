using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.Notifications.Dashboards
{
    public record LivingRoomPanelNotification
    {
        /// <summary>
        /// Gets or sets the main notification message.
        /// Serialized as 'm' in JSON.
        /// </summary>
        [JsonPropertyName("m")]
        public string Message { get; set; }

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
        /// Gets or initializes an optional timestamp string associated with the notification.
        /// Serialized as 't' in JSON.
        /// </summary>
        [JsonPropertyName("t")]
        public string? TimeStamp { get; init; }
    }
}

using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.Core.GoogleHome
{
    public class Alarm
    {
        [JsonPropertyName("alarm_id")]
        public string AlarmId { get; set; } = null!;

        [JsonPropertyName("fire_time")]
        public long FireTime { get; set; }

        [JsonPropertyName("local_time")]
        public string LocalTime { get; set; } = null!;

        [JsonPropertyName("local_time_iso")]
        public string LocalTimeIso { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("recurrence")]
        public int[]? Recurrence { get; set; } // Days of the week. Monday = 0, Sunday = 6.
    }
}

using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.GoogleHome
{
    public class Alarm
    {
        [JsonPropertyName("alarm_id")]
        public string AlarmId { get; set; }

        [JsonPropertyName("fire_time")]
        public long FireTime { get; set; }

        [JsonPropertyName("local_time")]
        public string LocalTime { get; set; }

        [JsonPropertyName("local_time_iso")]
        public string LocalTimeIso { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("recurrence")]
        public int[]? Recurrence { get; set; } // Days of the week. Monday = 0, Sunday = 6.
    }
}

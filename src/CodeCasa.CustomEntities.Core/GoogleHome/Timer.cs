using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.Core.GoogleHome
{
    public class Timer
    {
        [JsonPropertyName("timer_id")]
        public string TimerId { get; set; }

        [JsonPropertyName("fire_time")]
        public long FireTime { get; set; }

        [JsonPropertyName("local_time")]
        public string LocalTime { get; set; }

        [JsonPropertyName("local_time_iso")]
        public string LocalTimeIso { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }
    }
}

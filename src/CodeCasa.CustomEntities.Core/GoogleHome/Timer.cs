using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.Core.GoogleHome
{
    public class Timer
    {
        [JsonPropertyName("timer_id")]
        public string TimerId { get; set; } = null!;

        [JsonPropertyName("fire_time")]
        public long FireTime { get; set; }

        [JsonPropertyName("local_time")]
        public string LocalTime { get; set; } = null!;

        [JsonPropertyName("local_time_iso")]
        public string LocalTimeIso { get; set; } = null!;

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("label")]
        public string? Label { get; set; }
    }
}

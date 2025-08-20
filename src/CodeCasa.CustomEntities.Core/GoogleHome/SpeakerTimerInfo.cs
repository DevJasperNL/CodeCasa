using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.Core.GoogleHome;

public class SpeakerTimerInfo
{
    [JsonPropertyName("next_timer_status")]
    public string NextTimerStatus { get; set; } = null!;

    [JsonPropertyName("timers")]
    public List<Timer>? Timers { get; set; }

    [JsonPropertyName("device_class")]
    public string DeviceClass { get; set; } = null!;

    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; set; } = null!;
}
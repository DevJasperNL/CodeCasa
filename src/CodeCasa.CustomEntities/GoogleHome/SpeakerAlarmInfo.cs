using System.Text.Json.Serialization;

namespace CodeCasa.CustomEntities.GoogleHome;

public class SpeakerAlarmInfo
{
    [JsonPropertyName("next_alarm_status")]
    public string NextAlarmStatus { get; set; }

    [JsonPropertyName("alarm_volume")]
    public int AlarmVolume { get; set; }

    [JsonPropertyName("alarms")]
    public List<Alarm>? Alarms { get; set; }

    [JsonPropertyName("device_class")]
    public string DeviceClass { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("friendly_name")]
    public string FriendlyName { get; set; }
}
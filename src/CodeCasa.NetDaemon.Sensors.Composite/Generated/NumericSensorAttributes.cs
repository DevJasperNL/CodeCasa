using System.Text.Json.Serialization;

namespace CodeCasa.NetDaemon.Sensors.Composite.Generated;

internal partial record NumericSensorAttributes
{
    [JsonPropertyName("state_class")]
    public string? StateClass { get; init; }

    [JsonPropertyName("unit_of_measurement")]
    public string? UnitOfMeasurement { get; init; }

    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("Available")]
    public string? Available { get; init; }

    [JsonPropertyName("Available (Important)")]
    public string? AvailableImportant { get; init; }

    [JsonPropertyName("Available (Opportunistic)")]
    public string? AvailableOpportunistic { get; init; }

    [JsonPropertyName("Total")]
    public string? Total { get; init; }

    [JsonPropertyName("marker_high_level")]
    public double? MarkerHighLevel { get; init; }

    [JsonPropertyName("marker_low_level")]
    public double? MarkerLowLevel { get; init; }

    [JsonPropertyName("marker_type")]
    public string? MarkerType { get; init; }

    [JsonPropertyName("restored")]
    public bool? Restored { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }
}
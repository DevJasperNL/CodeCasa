using System.Text.Json.Serialization;

namespace CodeCasa.Lights.NetDaemon.Generated;

internal record LightAttributes
{
    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }

    [JsonPropertyName("brightness")]
    public double? Brightness { get; init; }

    [JsonPropertyName("color_temp_kelvin")]
    public double? ColorTempKelvin { get; init; }

    [JsonPropertyName("rgb_color")]
    public IReadOnlyList<double>? RgbColor { get; init; }

    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }
}
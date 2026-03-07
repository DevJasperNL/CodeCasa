using System.Text.Json.Serialization;

namespace CodeCasa.Lights.NetDaemon.Generated;

internal record LightTurnOnParameters
{
    [JsonPropertyName("transition")]
    public double? Transition { get; init; }

    ///<summary> eg: [255, 100, 100]</summary>
    [JsonPropertyName("rgb_color")]
    public IReadOnlyCollection<int>? RgbColor { get; init; }

    [JsonPropertyName("color_temp_kelvin")]
    public object? ColorTempKelvin { get; init; }

    [JsonPropertyName("brightness")]
    public double? Brightness { get; init; }
}
using System.Text.Json.Serialization;

namespace CodeCasa.Lights.NetDaemon.Generated;

internal record LightTurnOffParameters
{
    ///<summary>Duration it takes to get to next state.</summary>
    [JsonPropertyName("transition")]
    public double? Transition { get; init; }
}
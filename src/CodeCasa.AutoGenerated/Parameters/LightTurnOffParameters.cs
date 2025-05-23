﻿using System.Text.Json.Serialization;

namespace CodeCasa.AutoGenerated.Parameters;

public partial record LightTurnOffParameters
{
    ///<summary>Duration it takes to get to next state.</summary>
    [JsonPropertyName("transition")]
    public double? Transition { get; init; }

    [JsonPropertyName("flash")]
    public object? Flash { get; init; }
}
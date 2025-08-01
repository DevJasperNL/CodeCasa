﻿using System.Text.Json.Serialization;

namespace CodeCasa.AutoGenerated;

public partial record InputSelectAttributes
{
    [JsonPropertyName("options")]
    public IReadOnlyList<string>? Options { get; init; }

    [JsonPropertyName("editable")]
    public bool? Editable { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }
}
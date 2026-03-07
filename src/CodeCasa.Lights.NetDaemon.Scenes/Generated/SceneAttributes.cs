using System.Text.Json.Serialization;

namespace CodeCasa.Lights.NetDaemon.Scenes.Generated;

internal record SceneAttributes
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
}
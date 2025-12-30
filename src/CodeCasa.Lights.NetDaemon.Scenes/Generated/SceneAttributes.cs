using System.Text.Json.Serialization;

namespace CodeCasa.Lights.NetDaemon.Scenes.Generated;

internal record SceneAttributes
{
    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }
}
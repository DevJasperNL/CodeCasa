using System.Text.Json;
using System.Text.Json.Serialization;
using CodeCasa.Lights.NetDaemon.Generated;

namespace CodeCasa.Lights.NetDaemon.Scenes
{
    internal class SceneConfig
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("entities")]
        public IReadOnlyDictionary<string, object> Entities
        {
            get;
            set
            {
                field = value ?? throw new ArgumentNullException(nameof(value));

                Lights = field
                    .Where(ekv => ekv.Key.Split('.')[0] == "light")
                    .ToDictionary(ekv => ekv.Key, ekv => JsonSerializer.Deserialize<LightAttributes>(
                        JsonSerializer.Serialize(ekv.Value))!);
            }
        } = new Dictionary<string, object>();

        public IReadOnlyDictionary<string, LightAttributes> Lights { get; private set; } = new Dictionary<string, LightAttributes>();
    }
}

using StarfallAfterlife.Bridge.Serialization;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace StarfallAfterlife.Bridge.Database
{
    public struct HouseUpgradeLevelInfo
    {
        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("req_level")]
        public int RequiredLevel { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("params"), JsonConverter(typeof(JsonNodeStringConverter))]
        public JsonNode Params { get; set; }
    }
}
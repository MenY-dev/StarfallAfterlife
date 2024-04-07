using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public class SfCodexItem
    {
        [JsonPropertyName(name: "id")]
        public int Id { get; set; }

        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("base_class")]
        public string BaseClass { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("name_key")]
        public string NameKey { get; set; }

        [JsonPropertyName("description_key")]
        public string DescriptionKey { get; set; }

        [JsonPropertyName("fields")]
        public Dictionary<string, object> Fields { get; set; }
    }
}

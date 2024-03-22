using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public class SfLocalizationNamespace
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("strings")]
        public Dictionary<string, string> Strings { get; set; }
    }
}

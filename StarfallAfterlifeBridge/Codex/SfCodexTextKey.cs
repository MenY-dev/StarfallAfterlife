using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public struct SfCodexTextKey
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("namespace"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Namespace { get; set; }

        public SfCodexTextKey(string key, string @namespace = default)
        {
            Key = key;
            Namespace = @namespace;
        }
    }
}

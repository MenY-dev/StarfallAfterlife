using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ConditionProgress
    {
        [JsonPropertyName("prog")]
        public int Progress { get; set; }

        [JsonPropertyName("extra")]
        public Dictionary<string, int> ExtraOptions { get; set; } = new();

        public int GetOption(string name) =>
            ExtraOptions.GetValueOrDefault(name);

        public void SetOption(string name, int value) =>
            ExtraOptions[name ?? string.Empty] = value;
    }
}

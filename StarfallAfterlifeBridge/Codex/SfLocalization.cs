using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Codex
{
    public class SfLocalization
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("namespaces")]
        public List<SfLocalizationNamespace> Namespaces { get; set; }
    }
}

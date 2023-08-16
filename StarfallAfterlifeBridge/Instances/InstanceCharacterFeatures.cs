using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceCharacterFeatures
    {
        [JsonPropertyName("drop_rules")]
        public List<DiscoveryDropRule> DropRules { get; set; }
    }
}

using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceMob
    {
        [JsonPropertyName("mob_id")]
        public int Id { get; set; }

        [JsonPropertyName("mob_behavior_tree_name")]
        public string BehaviorTreeName { get; set; }

        [JsonPropertyName("mob_internal_name")]
        public string MobInternalName { get; set; }

        [JsonPropertyName("mob_faction")]
        public Faction MobFaction { get; set; }

        [JsonPropertyName("mob_tags")]
        public List<string> Tags { get; set; }

    }
}

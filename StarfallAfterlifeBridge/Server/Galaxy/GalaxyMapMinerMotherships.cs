using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapMinerMotherships : GalaxyMapStarSystemObject
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.MinerMothership;

        [JsonPropertyName("is_immortal")]
        public int Immortal { get; set; } = 1;
    }
}

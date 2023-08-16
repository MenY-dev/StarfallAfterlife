using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapScienceStation : GalaxyMapStarSystemObject
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.ScienceStation;
    }
}

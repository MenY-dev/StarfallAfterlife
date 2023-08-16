using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapPiratesOutpost : GalaxyMapPiratesStation
    {
        [JsonIgnore]
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.PiratesOutpost;
    }
}

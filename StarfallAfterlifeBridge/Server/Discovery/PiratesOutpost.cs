using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class PiratesOutpost : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.PiratesOutpost;
        public int Level { get; set; } = 0;

        public PiratesOutpost(GalaxyMapPiratesOutpost mapOutpost, StarSystem system)
        {
            System = system;
            Hex = mapOutpost.Hex;
            Id = mapOutpost.Id;
            Faction = (Faction)mapOutpost.Faction;
            FactionGroup = mapOutpost.FactionGroup;
            Level = mapOutpost.Level;
        }
    }
}

using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class PiratesStation : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.PiratesStation;
        public int Level { get; set; } = 0;

        public PiratesStation(GalaxyMapPiratesStation mapStation, StarSystem system)
        {
            System = system;
            Hex = mapStation.Hex;
            Id = mapStation.Id;
            Faction = (Faction)mapStation.Faction;
            FactionGroup = mapStation.FactionGroup;
            Level = mapStation.Level;
        }
    }
}

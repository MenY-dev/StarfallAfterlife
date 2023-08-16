using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class FuelStation : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.Fuelstation;

        public FuelStation(GalaxyMapFuelStation mapStation, StarSystem system)
        {
            System = system;
            Hex = mapStation.Hex;
            Id = mapStation.Id;
            Faction = (Faction?)system?.Info?.Faction ?? Faction.None;
            FactionGroup = system?.Info?.FactionGroup ?? 0;
        }
    }
}

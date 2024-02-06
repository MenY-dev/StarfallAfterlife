using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class SfaGameProfile
    {
        public ShipConstructionInfo AddRankedShip(int fleetId, int hull)
        {
            if (RankedFleets?.ToArray() is RankedFleetInfo[] rankedfleets &&
                rankedfleets.FirstOrDefault(f => f?.Id == fleetId) is RankedFleetInfo fleet)
            {
                var allShips = rankedfleets.SelectMany(f => f.Ships ?? new()).ToArray();
                var newId = Enumerable
                    .Range(1, allShips.Length + 1)
                    .FirstOrDefault(i => allShips.Any(s => s?.Id == i) == false, -1);

                if (newId > 0)
                {
                    var ship = new ShipConstructionInfo
                    {
                        Id = newId,
                        Hull = hull,
                        FleetId = fleetId,
                    };

                    (fleet.Ships ??= new()).Add(ship);
                    return ship;
                }
            }

            return null;
        }

        public bool DeleteRankedShip(int shipId)
        {
            foreach (var fleet in RankedFleets ?? new())
            {
                if (fleet?.Ships is List<ShipConstructionInfo> ships &&
                    ships.FirstOrDefault(s => s?.Id == shipId) is ShipConstructionInfo targetShip)
                {
                    return ships.Remove(targetShip);
                }
            }

            return false;
        }

        public ShipConstructionInfo GetRankedShip(int shipId)
        {
            return RankedFleets?
                .SelectMany(f => f?.Ships ?? new())
                .FirstOrDefault(s => s?.Id == shipId);
        }
    }
}

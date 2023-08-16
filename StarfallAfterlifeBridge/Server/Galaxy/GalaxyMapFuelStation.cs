using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapFuelStation : GalaxyMapStarSystemObject
    {
        public override GalaxyMapObjectType ObjectType { get; } = GalaxyMapObjectType.FuelStation;
    }
}

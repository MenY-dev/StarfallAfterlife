using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public enum SecretObjectType : byte
    {
        None = 0,
        Stash = 1,
        Resources = 2,
        AbandonedStation = 3,
        AbandonedShip = 4,
        CovertFleet = 5,
        ShipsGraveyard = 6,
        SpaceFarm = 7,
    }
}

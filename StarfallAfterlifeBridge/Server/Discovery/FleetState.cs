using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public enum FleetState : byte
    {
        None = 0,
        InGalaxy = 1,
        InBattle = 3,
        Destroyed = 4
    }
}

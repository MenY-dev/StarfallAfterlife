using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public enum FleetState : byte
    {
        WaitingGalaxy = 0,
        InGalaxy = 1,
        WaitingBattle = 2,
        InBattle = 3
    }
}

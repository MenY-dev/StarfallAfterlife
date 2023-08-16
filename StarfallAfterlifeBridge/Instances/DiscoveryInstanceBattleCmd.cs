using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public enum DiscoveryInstanceBattleCmd : byte
    {
        Started = 0,
        KeepAlive = 1,
        EnterFleet = 2,
        ExitFleet = 3,
        NoFleets = 4,
    }
}

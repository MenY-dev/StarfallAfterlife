using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public enum RouteMoveResult : byte
    {
        None = 0,
        WaypointReached = 1,
        TargetLocationReached = 2
    }
}

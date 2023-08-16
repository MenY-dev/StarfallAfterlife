using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum BattleGroundServerAction : byte
    {
        InstanceReady = 0,
        InstanceError = 1,
        StageUpdated = 2,
        InstanceDone = 3,
        UpdateTeamStateEvent = 4,
    }
}

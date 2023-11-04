using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum QuickMatchServerAction : byte
    {
        InstanceReady = 0,
        InstanceError = 1,
        StageUpdated = 2,
        UpdateSpecOpsTeam = 3,
    }
}

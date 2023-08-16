using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum FleetRecallState : byte
    {
        Unknow = 0,
        Started = 1,
        Done = 2,
        Failed = 3,
        Canceled = 4,
    }
}

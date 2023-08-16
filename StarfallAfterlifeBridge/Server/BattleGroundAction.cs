using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum BattleGroundAction : byte
    {
        ReadyToPlay = 0,
        Cancel = 1,
        FindMatch = 2,
    }
}

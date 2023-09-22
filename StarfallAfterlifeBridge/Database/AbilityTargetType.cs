using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum AbilityTargetType : byte
    {
        Self = 0x0,
        Target = 0x1,
        Passive = 0x2,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum SfaCharacterState : byte
    {
        InShipyard = 0,
        EnterToGalaxy = 1,
        InGalaxy = 2,
        InBattle = 3
    }
}

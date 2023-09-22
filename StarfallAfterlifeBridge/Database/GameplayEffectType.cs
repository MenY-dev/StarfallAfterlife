using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum GameplayEffectType : byte
    {
        Unknown = 0,
        Immortal = 1,
        SpeedBoost = 2,
        Vision = 3,
        SharedVision = 4,
        EngineNullifier = 5,
        Stealth = 6,
        BlindVision = 7,
        FuelStationBoost = 8,
        Mine = 9,
        Scan = 10,
        Siege = 11,
    }
}

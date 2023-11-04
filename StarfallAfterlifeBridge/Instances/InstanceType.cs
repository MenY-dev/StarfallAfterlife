using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public enum InstanceType :  byte
    {
        None = 0,
        DiscoveryBattle = 1,
        DiscoveryDungeon = 2,
        MothershipAssault = 3,
        StationAttack = 4,
    }
}
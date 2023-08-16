using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public interface IBattleInstanceListener
    {
        void OnMobDestroyed(int fleetId, MobKillInfo mobInfo);
    }
}

using StarfallAfterlife.Bridge.Server.Matchmakers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IBattleListener
    {
        void OnBattleStarted(StarSystemBattle battle);

        void OnBattleFleetAdded(StarSystemBattle battle, BattleMember newMember);

        void OnBattleFleetLeaving(StarSystemBattle battle, BattleMember member);

        void OnBattleFinished(StarSystemBattle battle);
    }
}

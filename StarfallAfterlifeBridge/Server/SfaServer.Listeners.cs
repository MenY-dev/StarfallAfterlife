using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer : IBattleListener
    {
        void IBattleListener.OnBattleStarted(StarSystemBattle battle) => Invoke(() =>
        {
            if (battle is null)
                return;

            var matchmakerBattle = Matchmaker.DiscoveryGameMode.CreateNewBattle(battle);
            matchmakerBattle.Start();
            SfaDebug.Print($"Battle Started! (Hex = {battle.Hex})");
        });

        void IBattleListener.OnBattleFleetAdded(StarSystemBattle battle, BattleMember newMember) => Galaxy.BeginPreUpdateAction(g =>
        {
            Matchmaker?.DiscoveryGameMode?.GetBattle(battle)?.AddToBattle(newMember);

            if (newMember.Fleet is UserFleet fleet)
            {
                GetCharacter(fleet)?.DiscoveryClient?.SendFleetAttacked(
                   battle.AttackerId,
                   battle.AttackerTargetType,
                   battle.Hex);
            }
        });

        void IBattleListener.OnBattleFleetLeaving(StarSystemBattle battle, BattleMember member) => Invoke(() =>
        {

        });

        void IBattleListener.OnBattleFinished(StarSystemBattle battle) => Invoke(() =>
        {

        });
    }
}

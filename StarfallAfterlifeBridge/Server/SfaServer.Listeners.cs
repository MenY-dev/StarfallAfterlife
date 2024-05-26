using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer : IBattleListener
    {
        void IBattleListener.OnBattleStarted(StarSystemBattle battle)
        {
            if (battle.IsUserAdded == true)
            {
                Invoke(() =>
                {
                    var matchmakerBattle = Matchmaker?.DiscoveryGameMode?.CreateNewBattle(battle);

                    if (matchmakerBattle is not null)
                    {
                        matchmakerBattle.Start();
                        SfaDebug.Print($"Battle started! (Hex = {battle.Hex})", GetType().Name);
                    }
                    else SfaDebug.Print($"Battle not started! (Reason: Matchmaker Error)", GetType().Name);
                });
            }
            else SfaDebug.Print($"Battle not started! (Reason: Waiting players)", GetType().Name);
        }

        void IBattleListener.OnBattleFleetAdded(StarSystemBattle battle, BattleMember newMember) => Invoke(() =>
        {
            var matchmakerBattle = Matchmaker?.DiscoveryGameMode?.GetBattle(battle);

            if (battle.IsStarted == true && battle.IsFinished == false &&
                matchmakerBattle is null && newMember.Fleet is UserFleet fleet)
            {
                matchmakerBattle = Matchmaker?.DiscoveryGameMode?.CreateNewBattle(battle);

                if (matchmakerBattle is not null)
                {
                    SfaDebug.Print($"Battle started! (Hex = {battle.Hex})", GetType().Name);

                    GetCharacter(fleet)?.DiscoveryClient?.SendFleetAttacked(
                       battle.AttackerId,
                       battle.AttackerTargetType,
                       battle.Hex);

                    matchmakerBattle.Start();
                }
            }

            if (matchmakerBattle is null)
            {
                SfaDebug.Print($"Member not added to battle! (Reason: Waiting Battle)", GetType().Name);
                return;
            }

            matchmakerBattle.AddToBattle(newMember);
        });

        void IBattleListener.OnBattleFleetLeaving(StarSystemBattle battle, BattleMember member) => Invoke(() =>
        {
            if (member.Fleet is DiscoveryAiFleet mob &&
                FleetIdInfo.IsDynamicMob(mob.Id) == true &&
                mob.UseRespawn == false)
            {
                UseDynamicMobs(dtb => dtb.Remove(mob.Id));
                Galaxy.BeginPreUpdateAction(_ => mob.System?.RemoveFleet(mob));
            }
        });

        void IBattleListener.OnBattleFinished(StarSystemBattle battle) => Invoke(() =>
        {
            Matchmaker?.DiscoveryGameMode?.GetBattle(battle)?.Stop();
        });
    }
}

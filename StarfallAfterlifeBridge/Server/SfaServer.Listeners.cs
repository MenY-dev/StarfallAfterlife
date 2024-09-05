using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServer : IBattleListener, IGalaxyListener
    {
        void IBattleListener.OnBattleStarted(StarSystemBattle battle)
        {
            if (battle.IsUserAdded == true)
            {
                Invoke(() =>
                {
                    var matchmakerBattle = Matchmaker?.DiscoveryGameMode?.GetBattle(battle);

                    if (matchmakerBattle is not null)
                    {
                        SfaDebug.Print($"The battle has already started!", GetType().Name);
                        return;
                    }

                    matchmakerBattle = Matchmaker?.DiscoveryGameMode?.CreateNewBattle(battle);

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

        void IBattleListener.OnBattleFleetAdded(StarSystemBattle battle, BattleMember newMember)
        {
            var isBattleReady = battle.IsStarted == true && battle.IsFinished == false;

            Invoke(() =>
            {
                var matchmakerBattle = Matchmaker?.DiscoveryGameMode?.GetBattle(battle);

                if (isBattleReady == true && matchmakerBattle is null && newMember.Fleet is UserFleet)
                {
                    matchmakerBattle = Matchmaker?.DiscoveryGameMode?.CreateNewBattle(battle);

                    if (matchmakerBattle is not null)
                    {
                        SfaDebug.Print($"Battle started! (Hex = {battle.Hex})", GetType().Name);
                        matchmakerBattle.Start();
                    }
                }

                if (newMember.Fleet is UserFleet user)
                {
                    GetCharacter(user)?.DiscoveryClient?.SendFleetAttacked(
                        battle.AttackerId,
                        battle.AttackerTargetType,
                        battle.Hex);
                }

                if (matchmakerBattle is null)
                {
                    SfaDebug.Print($"Member not added to battle! (Reason: Waiting Battle)", GetType().Name);
                    return;
                }

                SfaDebug.Print($"Member added to battle!", GetType().Name);
                matchmakerBattle.AddToBattle(newMember);
            });
        }

        void IBattleListener.OnBattleFleetLeaving(StarSystemBattle battle, BattleMember member) => Invoke(() =>
        {
            if (member.Fleet is DiscoveryAiFleet mob &&
                FleetIdInfo.IsDynamicMob(mob.Id) == true &&
                mob.UseRespawn == false)
            {
                RemoveDynamicMob(mob.Id);
                Galaxy.BeginPreUpdateAction(_ => mob.System?.RemoveFleet(mob));
            }
        });

        void IBattleListener.OnBattleFinished(StarSystemBattle battle) => Invoke(() =>
        {
            Matchmaker?.DiscoveryGameMode?.GetBattle(battle)?.Stop();
        });

        void IGalaxyListener.OnStarSystemActivated(StarSystem system)
        {
            SpawnBlockadeInSystem(system);
        }
    }
}

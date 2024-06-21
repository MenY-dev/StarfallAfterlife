using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class BlockadeAI : FleetAI
    {
        public SystemHex TargetHex { get; set; }

        public AIStateMachine StateMachine { get; protected set; }

        public AIState DefaultState { get; protected set; }

        public AIState CurrentState => StateMachine?.CurrentState;


        public override void Update()
        {
            if (IsConnected == true &&
                CurrentAction is not AIStateMachine or not { State: AINodeState.Started })
            {
                StartAction(StateMachine = CreateBehavior());
            }

            base.Update();
        }

        protected virtual AIStateMachine CreateBehavior()
        {
            var sm = new AIStateMachine();

            sm.States.Add(DefaultState = new()
            {
                Name = "idle",
                Default = true,
                Looped = true,
            });

            sm.States.Add(new()
            {
                Name = "attack_target",
                Action = (AIState s) => new AttackAction(
                    s.Context as StarSystemObject, TimeSpan.FromSeconds(4), 2),
            });

            sm.States.Add(new()
            {
                Name = "move_to_target_hex",
                Action = (AIState s) => new MoveToPointAction(SystemHexMap.HexToSystemPoint(TargetHex)),
            });

            sm.Watchdogs.Add(new()
            {
                Name = "find_enemy_watchdog",
                Action = FindEnemyWatchdog,
            });

            sm.Watchdogs.Add(new()
            {
                Name = "move_to_target_hex_watchdog",
                Action = MoveToTargetHexWatchdog,
                Period = TimeSpan.FromSeconds(1),
            });

            return sm;
        }

        private void MoveToTargetHexWatchdog(AIWatchdog watchdog)
        {
            var fleet = Fleet;

            if (IsConnected == false ||
                fleet is null ||
                fleet.State != FleetState.InGalaxy ||
                CurrentState != DefaultState ||
                fleet.AttackTarget is not null ||
                fleet.GetBattle() is not null ||
                fleet.Hex == TargetHex)
                return;

            StateMachine.StartStateByName("move_to_target_hex");
        }

        private void FindEnemyWatchdog(AIWatchdog watchdog)
        {
            var fleet = Fleet;

            if (IsConnected == false ||
                fleet is null ||
                fleet.State != FleetState.InGalaxy ||
                fleet.Hex != TargetHex ||
                CurrentState != DefaultState ||
                fleet.GetBattle() is not null)
                return;

            var faction = fleet.Faction;
            var enemies = fleet.System?.Fleets.Where(enemy =>
                enemy != fleet &&
                enemy.Hex == TargetHex &&
                enemy.State == FleetState.InGalaxy &&
                enemy.Faction.IsEnemy(faction, true) &&
                fleet.CanAttack(enemy) &&
                fleet.IsVisible(enemy, true) &&
                enemy.GetBattle() is null);

            if (enemies.FirstOrDefault() is DiscoveryFleet enemy)
                StateMachine.StartStateByName("attack_target", enemy);
        }
    }
}

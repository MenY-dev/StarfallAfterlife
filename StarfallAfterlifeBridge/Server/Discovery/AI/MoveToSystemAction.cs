using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class MoveToSystemAction : AIAction
    {
        public int TargetSystem { get; set; }

        public Queue<int> Path { get; } = new();

        public GalaxyMapPortal TargetPortal { get; set; }

        public MoveToSystemAction(int systemId)
        {
            TargetSystem = systemId;
        }

        public override void Start()
        {
            base.Start();
            Path.Clear();

            if (Fleet?.System is StarSystem currentSystem &&
                currentSystem.Galaxy is DiscoveryGalaxy galaxy &&
                galaxy.Map is GalaxyMap map)
            {
                if (currentSystem.Id == TargetSystem)
                {
                    State = AINodeState.Completed;
                    return;
                }

                var levels = Enumerable.Range(1, SfaDatabase.LevelToAccessLevel(Fleet.Level));
                var path = map.FindPath(currentSystem.Id, TargetSystem, levels.ToArray());

                if (path is null ||
                    path.Count < 1 ||
                    path.First() != currentSystem.Id ||
                    path.Last() != TargetSystem)
                {
                    State = AINodeState.Failed;
                    return;
                }

                foreach (var item in path)
                    Path.Enqueue(item);
            }
            else
            {
                State = AINodeState.Failed;
            }
        }

        public override void Update()
        {
            base.Update();

            if (State != AINodeState.Started)
                return;

            if (HandleCompletion() == true)
                return;

            if (Fleet is DiscoveryFleet fleet &&
                Fleet?.System is StarSystem currentSystem &&
                currentSystem.Galaxy is DiscoveryGalaxy galaxy &&
                fleet.State == FleetState.InGalaxy)
            {
                if (Path.Peek() == currentSystem.Id)
                    Path.Dequeue();

                if (HandleCompletion() == true)
                    return;

                var targetSystem = Path.Peek();
                var portel = currentSystem.WarpBeacons.FirstOrDefault(w => w.Destination == targetSystem);

                if (portel is null)
                {
                    State = AINodeState.Failed;
                    return;
                }

                if (fleet.Hex == portel.Hex)
                {
                    if (galaxy.GetActiveSystem(targetSystem, true) is StarSystem nextSystem)
                    {
                        var spawnHex = SystemHexMap.SystemPointToHex(SystemHexMap.HexToSystemPoint(portel.Hex).GetNegative());
                        spawnHex = nextSystem.GetNearestSafeHex(fleet, spawnHex, false);
                        galaxy.BeginPostUpdateAction(_ =>
                        {
                            nextSystem.AddFleet(fleet, SystemHexMap.HexToSystemPoint(spawnHex));
                            fleet.AddEffect(new() { Logic = GameplayEffectType.Immortal, Duration = 2 });
                        });
                        HandleCompletion();
                    }
                    else
                    {
                        State = AINodeState.Failed;
                    }

                    return;
                }
                else if (SystemHexMap.SystemPointToHex(fleet.TargetLocation) != portel.Hex)
                {
                    fleet.MoveTo(portel.Hex);
                    return;
                }
            }
            else
            {
                State = AINodeState.Failed;
                return;
            }
        }

        public bool HandleCompletion()
        {
            if ((Path.Count == 1 && Fleet?.System?.Id == Path.Peek()) || Path.Count < 1)
            {
                State = AINodeState.Completed;
                return true;
            }

            return false;
        }
    }
}

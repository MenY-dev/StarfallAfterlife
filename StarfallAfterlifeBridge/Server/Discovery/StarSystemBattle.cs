using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class StarSystemBattle : SfaObject
    {
        public Guid Id { get; set; }

        public StarSystem System { get; set; }

        public SystemHex Hex { get; set; }

        public bool IsStarted { get; set; }

        public bool IsFinished { get; set; }

        public bool IsDungeon { get; set; }

        public bool HasNebula { get; set; }

        public bool HasAsteroids { get; set; }

        public StarSystemRichAsteroid RichAsteroidField { get; set; }

        public int FloatingAsteroidsCount { get; set; }

        public DungeonInfo DungeonInfo { get; set; }

        public List<BattleMember> Members { get; } = new();

        protected DiscoveryGalaxy Galaxy => System?.Galaxy;

        public int AttackerId { get; protected set; } = -1;

        public DiscoveryObjectType AttackerTargetType { get; protected set; } = DiscoveryObjectType.None;

        public override void Init()
        {
            base.Init();

            if (Id == Guid.Empty)
                Id = Guid.NewGuid();
        }

        public virtual void Start()
        {
            IsStarted = true;

            foreach (var member in Members)
                member.Fleet.SetFleetState(FleetState.InBattle);

            foreach (var member in Members)
                Galaxy?.Listeners.Broadcast<IBattleListener>(l => l.OnBattleFleetAdded(this, member));

            UpdateAttackerInfo();

            Galaxy?.Listeners.Broadcast<IBattleListener>(l => l.OnBattleStarted(this));
        }

        public void UpdateAttackerInfo()
        {
            if (Members.FirstOrDefault(m => m.Role == BattleRole.Explore)?.Fleet is UserFleet exploreFleet)
            {
                AttackerId = exploreFleet.Id;
                AttackerTargetType = exploreFleet.Type;
            }
            else if (Members.FirstOrDefault(m => m.Role == BattleRole.Attack)?.Fleet is DiscoveryAiFleet aiFleet)
            {
                AttackerId = aiFleet.Id;
                AttackerTargetType = DiscoveryObjectType.UserFleet;
            }
            else if (Members.FirstOrDefault(m => m.Role == BattleRole.Attack)?.Fleet is UserFleet userFleet)
            {
                AttackerId = userFleet.Id;
                AttackerTargetType = IsDungeon ? DungeonInfo?.Target?.Type ?? DiscoveryObjectType.None : DiscoveryObjectType.AiFleet;
            }
            else if (Members.FirstOrDefault(m => m.Role != BattleRole.Attack)?.Fleet is StarSystemObject target)
            {
                AttackerId = target.Id;
                AttackerTargetType = target.Type;
            }
            else
            {
                AttackerId = -1;
                AttackerTargetType = DiscoveryObjectType.None;
            }
        }

        public virtual void Finish()
        {
            IsFinished = false;
            Galaxy?.Listeners.Broadcast<IBattleListener>(l => l.OnBattleFinished(this));

            foreach (var member in Members.ToList())
                Leave(member, Hex);

            System?.RemoveBattle(this);
        }

        public void AddToBattle(DiscoveryFleet fleet, BattleRole role, Vector2 hexOffset) =>
            AddToBattle(new BattleMember(fleet, role, hexOffset));

        public virtual void AddToBattle(BattleMember member)
        {
            if (member is null ||
                member.Fleet is null)
                return;

            Members.RemoveAll(m => m.Fleet == member.Fleet);

            if (Members.Contains(member) == false)
                Members.Add(member);

            if (IsStarted == true && IsFinished == false)
            {
                member.Fleet.SetFleetState(FleetState.InBattle);
                Galaxy?.Listeners.Broadcast<IBattleListener>(l => l.OnBattleFleetAdded(this, member));
            }
        }

        public virtual void Leave(DiscoveryFleet fleet, SystemHex spawnHex, bool destroyed = false) =>
            Leave(Members.FirstOrDefault(m => m.Fleet == fleet), spawnHex, destroyed);

        public virtual void Leave(BattleMember member, SystemHex spawnHex, bool destroyed = false)
        {
            if (member is not null && Members.Remove(member) == true && member.Fleet is DiscoveryFleet fleet)
            {
                if (fleet.System is StarSystem system)
                {
                    var safeHex = system.GetNearestSafeHex(fleet, spawnHex);
                    fleet.SetLocation(SystemHexMap.HexToSystemPoint(safeHex));
                }

                Galaxy?.Listeners.Broadcast<IBattleListener>(l => l.OnBattleFleetLeaving(this, member));

                if (destroyed == true)
                {
                    fleet.SetFleetState(FleetState.Destroyed);
                    fleet.System?.Broadcast<IStarSystemObjectListener>(l => l.OnObjectSpawned(member.Fleet));
                }
                else
                {
                    if (fleet is UserFleet)
                        fleet.AddEffect(new(){Duration = 10, Logic = GameplayEffectType.Immortal});

                    fleet.SetFleetState(FleetState.InGalaxy);
                    fleet.System?.Broadcast<IStarSystemObjectListener>(l => l.OnObjectSpawned(member.Fleet));
                }
            }

            if (IsStarted == true &&
                IsFinished == false &&
                IsDungeon == false &&
                Members.Count < 1)
                Finish();
        }


        public void SetDungeonCompleted()
        {
            if (IsDungeon == true &&
                DungeonInfo.Target is StarSystemDungeon dungeon)
                dungeon.SetDungeonVisible(false);
        }

        public virtual BattleMember GetMember(DiscoveryFleet fleet) =>
            Members?.FirstOrDefault(m => m.Fleet == fleet);

        public bool IsInBattle(StarSystemObject obj) => 
            obj is not null &&
            (
                GetMember(obj as DiscoveryFleet) is not null ||
                DungeonInfo?.Target == obj
            );
    }
}

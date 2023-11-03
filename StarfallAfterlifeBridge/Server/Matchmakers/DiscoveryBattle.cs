using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class DiscoveryBattle : MatchmakerBattle
    {
        public int SystemId { get; set; }

        public SystemHex Location { get; set; }

        public StarSystemBattle SystemBattle { get; set; }

        public DiscoveryGalaxy Galaxy => SystemBattle?.System?.Galaxy;

        public List<DiscoveryBattleCharacterInfo> Characters { get; } = new();

        public List<DiscoveryBattleMobInfo> Mobs { get; } = new();

        public List<DiscoveryBattleBossInfo> Bosses { get; } = new();

        public List<BattleMember> PendingMembers { get; } = new();

        protected CancellationTokenSource _cts;
        protected readonly object _lockher = new();

        public virtual void AddToBattle(BattleMember member)
        {
            lock (_lockher)
            {
                if (member?.Fleet is null ||
                    PendingMembers.FirstOrDefault(m => m.Fleet == member.Fleet) is not null)
                    return;

                if (State == MatchmakerBattleState.PendingMatch)
                {
                    PendingMembers.Add(member);
                    return;
                }
                else if (State == MatchmakerBattleState.Started)
                {
                    AddToActiveBattle(member);
                    return;
                }
                else if (State == MatchmakerBattleState.Finished)
                {
                    Galaxy?.BeginPreUpdateAction(g => SystemBattle?.Leave(member, SystemBattle.Hex));
                    return;
                }

                if (member.Fleet is UserFleet)
                {
                    var info = CreateCharacterInfo(member);

                    if (info is not null)
                    {
                        Characters.Add(info);

                        if (State == MatchmakerBattleState.PendingPlayers)
                        {
                            _cts?.Cancel();
                            StartInstance();
                        }
                    }
                }
                else if (member.Fleet is DiscoveryAiFleet)
                {
                    var info = CreateMobInfo(member);

                    if (info is not null)
                        Mobs.Add(info);
                }
            }
        }

        protected virtual void AddToActiveBattle(BattleMember member)
        {
            lock (_lockher)
            {
                if (member?.Fleet is null)
                    return;

                if (member.Fleet is UserFleet)
                {
                    var info = Characters.FirstOrDefault(c => c.Member.Fleet == member.Fleet);

                    if (info is not null)
                    {
                        info.Member = member;
                        JoinToInstance(info);
                    }
                    else if ((info = CreateCharacterInfo(member)) is not null)
                    {
                        Characters.Add(info);
                        GameMode.InstanceManager.JoinNewChar(InstanceInfo, info.InstanceCharacter);
                    }
                }
                else if (member.Fleet is DiscoveryAiFleet)
                {
                    var info = CreateMobInfo(member);

                    if (info is not null)
                        Mobs.Add(info);
                }
            }
        }

        public void Leave(DiscoveryBattleCharacterInfo character, SystemHex spawnHex)
        {
            lock (_lockher)
            {
                Characters.Remove(character);
                Galaxy?.BeginPreUpdateAction(g => SystemBattle?.Leave(character.Member, spawnHex));
            }
        }

        public void Leave(DiscoveryBattleMobInfo mob, SystemHex spawnHex)
        {
            lock (_lockher)
            {
                Mobs.Remove(mob);
                Galaxy?.BeginPreUpdateAction(g => SystemBattle?.Leave(mob.Member, spawnHex));
            }
        }

        protected virtual DiscoveryBattleCharacterInfo CreateCharacterInfo(BattleMember member)
        {
            DiscoveryBattleCharacterInfo info = null;

            if (member.Fleet is UserFleet fleet)
            {
                if (Server?.GetCharacter(fleet) is ServerCharacter character)
                {
                    info = new DiscoveryBattleCharacterInfo(member, character);
                }
            }

            return info;
        }

        protected virtual DiscoveryBattleMobInfo CreateMobInfo(BattleMember member)
        {
            DiscoveryBattleMobInfo info = null;

            if (member.Fleet is DiscoveryAiFleet fleet)
            {
                if (Server?.Realm?.MobsDatabase?.GetMob(fleet.MobId) is DiscoveryMobInfo mobInfo)
                {
                    var mob = mobInfo?.Clone();
                    int fleetId = fleet.Id;
                    int shipId = fleetId * 1000;

                    foreach (var ship in mob.Ships?
                        .Select(s => s.Data)
                        .Where(s => s is not null) ??
                        Enumerable.Empty<ShipConstructionInfo>())
                    {
                        ship.Id = shipId;
                        ship.FleetId = fleetId;
                        shipId++;
                    }

                    info = new DiscoveryBattleMobInfo(member, mobInfo);
                }
            }

            return info;
        }


        protected virtual DiscoveryBattleBossInfo CreateBossInfo(GalaxyMapMob fleet)
        {
            if (fleet is null)
                return null;

            if (Server?.Realm?.MobsDatabase?.GetMob(fleet.MobId) is DiscoveryMobInfo mobInfo)
            {
                var mob = mobInfo.Clone();
                int fleetId = fleet.FleetId;
                int shipId = fleetId * 1000;

                foreach (var ship in mob.Ships?
                    .Select(s => s.Data)
                    .Where(s => s is not null) ??
                    Enumerable.Empty<ShipConstructionInfo>())
                {
                    ship.Id = shipId;
                    ship.FleetId = fleetId;
                    shipId++;
                }


                return new DiscoveryBattleBossInfo(fleetId, mob);
            }

            return null;
        }

        protected virtual DiscoveryBattleBossInfo CreateBossInfo(DiscoveryMobInfo mobInfo)
        {
            if (mobInfo is null)
                return null;

            int fleetId = 2000000;

            foreach (var item in Bosses ?? new())
                if (item is not null)
                    fleetId = Math.Max(fleetId, item.FleetId);

            fleetId++;

            var mob = mobInfo.Clone();
            int shipId = fleetId * 1000;

            foreach (var ship in mob.Ships?
                .Select(s => s.Data)
                .Where(s => s is not null) ??
                Enumerable.Empty<ShipConstructionInfo>())
            {
                ship.Id = shipId;
                ship.FleetId = fleetId;
                shipId++;
            }

            return new DiscoveryBattleBossInfo(fleetId, mob);
        }

        public override void Init()
        {
            base.Init();

            lock (_lockher)
            {
                InstanceInfo.Type = InstanceType.DiscoveryBattle;

                if (SystemBattle is null)
                    return;

                SystemId = SystemBattle.System?.Id ?? -1;
                Location = SystemBattle.Hex;

                foreach (var member in SystemBattle.Members)
                {
                    AddToBattle(member);
                }
            }
        }

        public override void Start()
        {
            lock (_lockher)
            {
                if (Characters.Count > 0)
                {
                    StartInstance();
                }
                else
                {
                    State = MatchmakerBattleState.PendingPlayers;
                    _cts = new();

                    Matchmaker.Invoke(() =>
                    {
                        if (_cts?.IsCancellationRequested == true)
                            return;

                        Stop();
                    }, TimeSpan.FromMinutes(1));
                }
            }
        }

        protected virtual void StartInstance()
        {
            lock (_lockher)
            {
                State = MatchmakerBattleState.PendingMatch;
                InstanceInfo.Characters.AddRange(Characters.Select(c => c.InstanceCharacter));
                InstanceInfo.ExtraData = CreateExtraData().ToJson().ToJsonString();
                GameMode.InstanceManager.StartInstance(InstanceInfo);
            }
        }

        public override void Stop()
        {
            lock (_lockher)
            {
                State = MatchmakerBattleState.Finished;

                foreach (var item in PendingMembers.ToArray())
                    Galaxy?.BeginPreUpdateAction(g => SystemBattle?.Leave(item, SystemBattle.Hex, false));

                PendingMembers.Clear();

                foreach (var character in Characters.ToArray())
                    Leave(character, SystemBattle.Hex);

                foreach (var mob in Mobs.ToArray())
                    Leave(mob, SystemBattle.Hex);

                Bosses?.Clear();

                GameMode.InstanceManager.RemoveInstance(InstanceInfo);
                Matchmaker.RemoveBattle(this);
                Galaxy?.BeginPreUpdateAction(g => SystemBattle.Finish());
            }
        }

        protected virtual InstanceExtraData CreateExtraData()
        {
            return new InstanceExtraData
            {
                AiList = Mobs?.Select(m => m.InstanceFleet).ToList() ?? new(),
                Bosses = Bosses?.Select(b => b.InstanceMob).ToList() ?? new(),
                EnviropmentInfo = CreateEnviropment(),
            };
        }

        protected virtual InstanceEnviropmentInfo CreateEnviropment()
        {
            var env = new InstanceEnviropmentInfo();

            if (SystemBattle is StarSystemBattle battle)
            {
                env.PosX = battle.Hex.X;
                env.PosY = battle.Hex.Y;
                env.DistanceFactor = 1;
                env.SectorLockSeconds = 15;
                env.FloatingAsteroidsCount = battle.FloatingAsteroidsCount;

                if (battle.System is StarSystem system &&
                    system.Info is GalaxyMapStarSystem systemInfo)
                {
                    env.SystemLevel = systemInfo.Level;
                    env.StarId = systemInfo.Id;
                    env.StarSize = systemInfo.Size;
                    env.StarWeight = systemInfo.Weight;
                    env.StarTemp = systemInfo.Temperature;
                    env.StarType = systemInfo.Type;
                    env.EnviropmentEffects = new()
                    {
                        //"star_flashes",
                        //"plasma_storm",
                    };
                }

                if (battle.IsDungeon == false)
                {
                    var rnd = new Random();
                    env.HasNebula = battle.HasNebula;

                    if (battle.HasAsteroids == true)
                    {
                        env.FloatingAsteroidsCount = 0;
                        env.RichAsteroidsId = battle.RichAsteroidField?.Id ?? -1;

                        foreach (var item in battle.RichAsteroidField?.Ores ?? new())
                            env.AsteroidsContent[item] = rnd.Next(15, 31);
                    }
                }
            }

            return env;
        }

        public DiscoveryBattleCharacterInfo GetCharacter(ServerCharacter character)
        {
            return Characters.FirstOrDefault(c => c.ServerCharacter == character);
        }

        public override void InstanceStateChanged(InstanceState state)
        {
            base.InstanceStateChanged(state);

            lock (_lockher)
            {
                if (state == InstanceState.Started)
                {
                    State = MatchmakerBattleState.Started;

                    foreach (var member in PendingMembers)
                        AddToActiveBattle(member);

                    PendingMembers.Clear();

                    foreach (var character in Characters)
                        JoinToInstance(character);
                }
                else if (state == InstanceState.Finished)
                {
                    Stop();
                }
            }
        }

        public void OnFleetLeavesFromInstance(DiscoveryObjectType fleetType, int fleetId, SystemHex hex)
        {
            lock (_lockher)
            {
                if (fleetType is DiscoveryObjectType.UserFleet)
                {
                    var character = Characters?.FirstOrDefault(c => c.InstanceCharacter.Id == fleetId);
                    Leave(character, (SystemBattle?.Hex ?? SystemHex.Zero) + hex);
                }
                else if (fleetType is DiscoveryObjectType.AiFleet)
                {
                    var mob = Mobs?.FirstOrDefault(c => c.FleetId == fleetId);
                    Leave(mob, (SystemBattle?.Hex ?? SystemHex.Zero) + hex);
                }
            }
        }

        public ServerCharacter GetCharByShipId(int shipId)
        {
            lock ( _lockher)
            {
                return Characters
                    .Select(c => c?.ServerCharacter)
                    .FirstOrDefault(c => c?.GetShipById(shipId) is not null);
            }
        }

        public virtual JsonNode GetMobData(int fleetId)
        {
            lock ( _lockher)
            {
                var mob =
                Mobs?.FirstOrDefault(m => m?.FleetId == fleetId)?.Mob ??
                Bosses?.FirstOrDefault(b => b?.FleetId == fleetId)?.Mob;

                if (mob is not null)
                {
                    var ships = new JsonArray();

                    foreach (var ship in mob.Ships ?? Enumerable.Empty<DiscoveryMobShipData>())
                    {
                        if (ship?.Data is null)
                            continue;

                        ships.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(ship.Data.Id),
                            ["data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(ship.Data).ToJsonString(false)),
                            ["service_data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(ship.ServiceData).ToJsonString(false)),
                        });
                    }

                    var doc = new JsonObject
                    {
                        ["id"] = SValue.Create(fleetId),
                        ["level"] = SValue.Create(mob.Level),
                        ["faction"] = SValue.Create((byte)mob.Faction),
                        ["internal_name"] = SValue.Create(mob.InternalName),
                        ["battle_bt"] = SValue.Create(mob.BehaviorTreeName),
                        ["tags"] = mob.Tags?.Select(SValue.Create).ToJsonArray(),
                        ["ships"] = ships,
                    };

                    return doc;
                }

                return null;
            }
        }

        public virtual void OnInstanceAuthReady(int characterId, string auth)
        {
            if (Characters?
                .FirstOrDefault(c => c?.InstanceCharacter?.Id == characterId)
                is DiscoveryBattleCharacterInfo info)
                JoinToInstance(info, auth);
        }

        public void JoinToInstance(DiscoveryBattleCharacterInfo character, string newAuth = null)
        {
            lock (_lockher)
            {
                if (character is null)
                    return;

                if (newAuth is not null &&
                    character.InstanceCharacter is InstanceCharacter instanceCharacter)
                    instanceCharacter.Auth = newAuth;

                character.Client?.Invoke(c => c.SendStartBattle(
                    "discovery",
                    Matchmaker?.CreateBattleIpAddress(),
                    InstanceInfo?.Port ?? -1,
                    character.InstanceCharacter?.Auth,
                    character.ServerCharacter?.Fleet?.System.Id ?? 0,
                    character.ServerCharacter?.Fleet?.Id ?? -1));
            }
        }

        public void OnMobDestroyed(string data)
        {
            lock (_lockher)
            {
                if (JsonNode.Parse(data) is JsonObject doc &&
                (DiscoveryObjectType?)(byte?)doc["killer_type"] == DiscoveryObjectType.UserFleet &&
                (int?)doc["killer_id"] is int fleetId &&
                (int?)doc["mob_id"] is int mobFleetId &&
                Server.GetCharacter(fleetId) is ServerCharacter character)
                {
                    var battleMob = Mobs?.FirstOrDefault(m => m?.FleetId == mobFleetId);

                    if (battleMob is not null)
                        SystemBattle.Leave(battleMob.Member, Location, true);

                    var mobInfo = battleMob?.Mob ?? Bosses?.FirstOrDefault(b => b?.FleetId == mobFleetId)?.Mob;
                    var killInfo = new MobKillInfo();

                    killInfo.SystemId = SystemId;
                    killInfo.MobId = mobInfo?.Id ?? -1;
                    killInfo.FleetId = mobFleetId;
                    killInfo.ObjectId = (int?)doc["obj_id"] ?? -1;
                    killInfo.ObjectType = (DiscoveryObjectType?)(byte?)doc["type"] ?? DiscoveryObjectType.None;
                    killInfo.Faction = (Faction?)(byte?)doc["faction"] ?? Faction.None;
                    killInfo.FactionGroup = (int?)doc["faction_group"] ?? -1;
                    killInfo.ShipClass = (int?)doc["ship_class"] ?? 0;
                    killInfo.Level = (int?)doc["level"] ?? 0;
                    killInfo.RepEarned = (int?)doc["rep_earned"] ?? 0;
                    killInfo.IsInAttackEvent = (int?)doc["is_in_attack_event"] ?? 0;
                    killInfo.Tags.AddRange(doc["tags"]?.DeserializeUnbuffered<List<string>>() ?? new());

                    character.Events?.Broadcast<IBattleInstanceListener>(l =>
                        l.OnMobDestroyed(fleetId, killInfo));
                }
            }
        }

        public void UpdateCharacterStats(string data)
        {
            if (JsonNode.Parse(data) is JsonObject doc &&
                (int?)doc["char_id"] is int charId &&
                Server.GetCharacter(charId) is ServerCharacter character)
            {
                var stats = new Dictionary<string, float>();

                foreach (var item in doc["stats"]?.AsArray() ?? new())
                {
                    if ((string)item["stat"] is string stat &&
                        (float?)item["value"] is float value)
                        stats[stat] = value;
                }

                character.AddNewStats(stats);
            }
        }

        internal void OnPiratesAssaultStatusUpdated(string data)
        {
            if (JsonHelpers.ParseNodeUnbuffered(data) is JsonNode doc &&
                (int?)doc["destroyed"] == 1 &&
                SystemBattle.IsDungeon &&
                SystemBattle.DungeonInfo?.Target is StarSystemObject obj &&
                (DiscoveryObjectType?)(byte?)doc["obj_type"] == obj.Type &&
                (int?)doc["obj_id"] == obj.Id )
                SystemBattle.SetDungeonCompleted();
        }
    }
}

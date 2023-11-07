using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class StationAttackBattle : MatchmakerBattle
    {
        public List<QuickMatchCharInfo> Characters { get; } = new();

        public List<DiscoveryMobInfo> Mobs { get; } = new();

        public byte Difficulty { get; internal set; }

        protected List<CharacterReward> Rewards { get; } = new();

        private readonly object _lockher = new();

        public override void Start()
        {
            base.Start();

            lock (_lockher)
            {
                CreateRewards();

                foreach (var item in Characters)
                {
                    item?.Char?.DiscoveryClient.SendQuickMatchState(MatchMakingStage.StartingInstance);
                }

                State = MatchmakerBattleState.PendingMatch;
                
                InstanceInfo.Type = InstanceType.StationAttack;
                InstanceInfo.Characters.AddRange(Characters.Select(c => c.InstanceCharacter));
                InstanceInfo.ExtraData = CreateExtraData().ToJson().ToJsonString();
                GameMode.InstanceManager.StartInstance(InstanceInfo);
            }
        }

        public override void Stop()
        {
            base.Stop();

            lock (_lockher)
            {
                Matchmaker?.InstanceManager?.StopInstance(InstanceInfo);
            }
        }

        protected virtual InstanceExtraData CreateExtraData()
        {
            return new InstanceExtraData
            {
                NoPlayersLifetime = 10,
                SpecOpsDifficulty = Difficulty,
                ParentObjId = 1000000,
                ParentObjType = (int)DiscoveryObjectType.PiratesOutpost,
                ParentObjGroup = 0,
                ParentObjLvl = 7,
            };
        }

        public override void InstanceStateChanged(InstanceState state)
        {
            base.InstanceStateChanged(state);

            lock (_lockher)
            {
                if (state == InstanceState.Started)
                {
                    State = MatchmakerBattleState.Started;

                    foreach (var character in Characters)
                        JoinToInstance(character);
                }
                else if (state == InstanceState.Finished)
                {
                    Stop();
                }
            }
        }

        public void JoinToInstance(QuickMatchCharInfo character)
        {
            lock (_lockher)
            {
                if (character is null)
                    return;

                character.Char.DiscoveryClient?.Invoke(c => c.SendStartBattle(
                    "quick_match",
                    Matchmaker?.CreateBattleIpAddress(),
                    InstanceInfo?.Port ?? -1,
                    character.InstanceCharacter?.Auth,
                    -1,
                    -1));
            }
        }

        public JsonNode GetMobData(int mobId, Faction faction, string[] tags)
        {
            lock (_lockher)
            {
                tags ??= new string[0];

                var mob = Server.Realm.MobsDatabase.Mobs.FirstOrDefault(
                    m => m.Value?.Faction == faction &&
                    tags.All(t => m.Value?.Tags.Contains(t, StringComparer.InvariantCultureIgnoreCase) == true)).Value;

                mob ??= Server.Realm.MobsDatabase.Mobs.FirstOrDefault().Value;

                if (mob is not null)
                {
                    Mobs.Add(mob);
                    var ships = new JsonArray();
                    var shipId = 1000000000 + Mobs.Count * 1000;

                    foreach (var ship in mob.Ships ?? Enumerable.Empty<DiscoveryMobShipData>())
                    {
                        if (ship?.Data is null)
                            continue;

                        var shipData = ship.Data.Clone();
                        shipData.Id = shipId++;

                        ships.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(shipData.Id),
                            ["data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(shipData).ToJsonString(false)),
                            ["service_data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(ship.ServiceData).ToJsonString(false)),
                        });
                    }

                    var doc = new JsonObject
                    {
                        ["id"] = SValue.Create(mobId),
                        ["level"] = SValue.Create(mob.Level),
                        ["faction"] = SValue.Create((byte)mob.Faction),
                        ["internal_name"] = SValue.Create(""),
                        ["battle_bt"] = SValue.Create(mob.BehaviorTreeName),
                        ["tags"] = mob.Tags?.Select(SValue.Create).ToJsonArray(),
                        ["ships"] = ships,
                    };

                    return doc;
                }

                return null;
            }
        }

        public void OnFleetLeavesFromInstance(DiscoveryObjectType fleetType, int fleetId, SystemHex hex)
        {
            if (fleetType is DiscoveryObjectType.UserFleet)
            {
                Characters.RemoveAll(c => c.InstanceCharacter.Id == fleetId);

                if (Characters.Count < 1)
                    Stop();
            };
        }

        public override void CharStatusChanged(ServerCharacter character, UserInGameStatus status)
        {
            if (status != UserInGameStatus.CharInBattle)
            {
                Characters.RemoveAll(c => c.Char == character);

                if (Characters.Count < 1)
                    Stop();
            }
        }

        public override bool ContainsChar(ServerCharacter character)
        {
            if (character is null)
                return false;

            lock (_lockher)
                return Characters?.Any(c => c?.Char == character) == true;
        }

        protected void CreateRewards()
        {
            lock (_lockher)
            {
                Rewards.Clear();

                foreach (var reward in Server?.Realm?.CharacterRewardDatabase?.GetStationAttackRewards() ?? new())
                {
                    foreach (var character in Characters.ToArray().Select(c => c.Char))
                    {
                        if (character is null ||
                            character.CheckReward(reward.Id) == true)
                            continue;

                        Rewards.Add(new()
                        {
                            Character = character.UniqueId,
                            Id = reward.Id,
                            RewardId = reward.RewardId,
                            Count = reward.Count,
                        });
                    }
                }
            }
        }

        public override CharacterReward[] GetRewards() => Rewards.ToArray();
    }
}

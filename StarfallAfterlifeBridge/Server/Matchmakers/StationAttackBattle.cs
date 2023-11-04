using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Serialization;
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

        public byte Difficulty { get; internal set; }

        private readonly object _lockher = new();

        public override void Start()
        {
            base.Start();

            lock (_lockher)
            {
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

        protected List<DiscoveryMobInfo> AddedMobs { get; } = new();

        public JsonNode GetMobData(int mobId, Faction faction, string[] tags)
        {
            lock (_lockher)
            {
                var mob = Server.Realm.MobsDatabase.Mobs.FirstOrDefault(
                    m => m.Value?.Faction == faction).Value;

                mob ??= Server.Realm.MobsDatabase.Mobs.FirstOrDefault().Value;

                if (mob is not null)
                {
                    AddedMobs.Add(mob);
                    var ships = new JsonArray();
                    var shipId = 1000000000 + AddedMobs.Count * 1000;

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
                        ["internal_name"] = SValue.Create(mob.InternalName),
                        ["battle_bt"] = SValue.Create(mob.BehaviorTreeName),
                        ["tags"] = tags?.Select(SValue.Create).ToJsonArray(),
                        ["ships"] = ships,
                    };

                    return doc;
                }

                return null;
            }
        }
    }
}

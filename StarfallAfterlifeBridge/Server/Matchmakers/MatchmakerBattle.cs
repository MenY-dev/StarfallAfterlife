using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class MatchmakerBattle
    {
        public MatchmakerGameMode GameMode { get; set; }

        public SfaMatchmaker Matchmaker => GameMode?.Matchmaker;

        public SfaServer Server => GameMode?.Server;

        public MatchmakerBattleState State { get; set; } = MatchmakerBattleState.Created;

        public InstanceInfo InstanceInfo { get; } = new();

        public List<DiscoveryMobInfo> RequestedSpecialFleets { get; } = new();

        private readonly object _specialFleetslockher = new();

        public virtual void Init()
        {
            InstanceInfo.UsePortForwarding = Server?.UsePortForwarding ?? false;
        }

        public virtual void Start()
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void InstanceStateChanged(InstanceState state)
        {

        }

        public virtual void UserStatusChanged(SfaServerClient user, UserInGameStatus status)
        {

        }

        public virtual void CharStatusChanged(ServerCharacter character, UserInGameStatus status)
        {

        }

        public virtual bool ContainsChar(ServerCharacter character)
        {
            return false;
        }

        public virtual bool ContainsUser(SfaServerClient user)
        {
            return false;
        }

        public virtual CharacterReward[] GetRewards()
        {
            return Array.Empty<CharacterReward>();
        }

        public virtual JsonArray GetDropList(string dropName)
        {
            return null;
        }

        public virtual JsonNode GetMobData(MobDataRequest request)
        {
            return null;
        }

        public virtual JsonArray GetSpecialFleet(string fleetName)
        {
            var ships = new JsonArray();

            lock (_specialFleetslockher)
            {
                if (Server?.Realm?.MobsDatabase?.GetMob(fleetName) is DiscoveryMobInfo mob)
                {
                    int fleetId = 2000000 + RequestedSpecialFleets.Count;
                    int shipId = fleetId * 1000;

                    foreach (var item in mob.Ships ?? Enumerable.Empty<DiscoveryMobShipData>())
                    {
                        var data = item.Data.Clone();

                        data.Id = shipId;
                        data.FleetId = fleetId;

                        ships.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(shipId),
                            ["data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(data).ToJsonString(false)),
                            ["service_data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(item.ServiceData).ToJsonString(false)),
                        });

                        shipId++;
                    }

                    RequestedSpecialFleets.Add(mob);
                }
            }

            return ships;
        }

        public virtual void HandleBattleResults(JsonNode doc)
        {

        }
    }
}

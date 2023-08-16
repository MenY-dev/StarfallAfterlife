using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class SfaMatchmaker
    {
        public SfaServer Server { get; set; }

        public Uri InstanceManagerAddress { get; set; }

        public InstanceManagerClient InstanceManager { get; protected set; }

        public DiscoveryGameMode DiscoveryGameMode { get; protected set; }

        public MothershipAssaultGameMode MothershipAssaultGameMode { get; protected set; }

        public HashSet<MatchmakerGameMode> GameModes { get; } = new();

        public HashSet<MatchmakerBattle> Battles { get; } = new();

        protected object Lockher { get; } = new();

        public virtual void Start()
        {
            Stop();

            DiscoveryGameMode ??= new DiscoveryGameMode();
            MothershipAssaultGameMode ??= new MothershipAssaultGameMode();
            GameModes.Add(DiscoveryGameMode);
            GameModes.Add(MothershipAssaultGameMode);
            DiscoveryGameMode.Init(this);
            MothershipAssaultGameMode.Init(this);

            InstanceManager = new InstanceManagerClient();
            InstanceManager.CharacterDataRequested += CharacterDataRequested;
            InstanceManager.MobDataRequested += MobDataRequested;
            InstanceManager.SpecialFleetRequested += SpecialFleetRequested; ;
            InstanceManager.InstanceStateChanged += InstanceStateChanged;
            InstanceManager.InstanceAuthReady += InstanceAuthReady; ;
            InstanceManager.FleetLeaves += FleetLeaves;
            InstanceManager.ShipStatusUpdated += ShipStatusUpdated;
            InstanceManager.AddCharacterShipsXp += AddCharacterShipsXp;
            InstanceManager.NewInstanceAction += OnNewInstanceAction;
            InstanceManager.ConnectAsync(InstanceManagerAddress).Wait();

            SfaDebug.Print($"Matchmaker Started! (InstanceManagerAddress = {InstanceManagerAddress})");
        }

        public void Stop()
        {
            InstanceManager?.Close();
        }

        protected virtual void CharacterDataRequested(object sender, CharacterDataRequestEventArgs e)
        {
            if (Server.Characters.TryGetValue(e.CharacterId, out var character) == true)
            {
                if (e.GameMode == "battlegrounds")
                {
                    character.DiscoveryClient?
                        .RequestDiscoveryCharacterData()
                        .ContinueWith(t => InstanceManager.SendCharacterData(e.CharacterId, JsonNode.Parse(t.Result?.ToJsonString())));
                }
                else
                {
                    InstanceManager.SendCharacterData(e.CharacterId, character.ToDiscoveryCharacterData());
                }
            }
        }

        private void MobDataRequested(object sender, MobDataRequestEventArgs e)
        {
            if (e.InstanceAuth is not null &&
                Battles.FirstOrDefault(b => b.InstanceInfo?.Auth == e.InstanceAuth) is DiscoveryBattle battle &&
                battle.GetMobData(e.MobId) is JsonNode doc)
            {
                InstanceManager.SendMobData(e.InstanceAuth, e.MobId, doc);
            }
        }

        private void SpecialFleetRequested(object sender, SpecialFleetRequestEventArgs e)
        {

            if (Server?.Realm?.MobsDatabase?.GetMob(e.FleetName) is DiscoveryMobInfo mob)
            {
                var ships = new JsonArray();
                int shipId = 2000000000;

                foreach (var item in mob.Ships ?? Enumerable.Empty<DiscoveryMobShipData>())
                {
                    ships.Add(new JsonObject
                    {
                        ["id"] = new SValue(shipId),
                        ["data"] = new SValue(JsonNode.Parse(item.Data).ToJsonString(false)),
                        ["service_data"] = new SValue(JsonNode.Parse(item.ServiceData).ToJsonString(false)),
                    });

                    shipId++;
                }

                InstanceManager.SendSpecialFleetData(e.FleetName, e.Auth, ships);
            }
        }

        protected virtual void InstanceStateChanged(object sender, InstanceInfoEventArgs e)
        {
            lock (Lockher)
            {
                Battles
                    .FirstOrDefault(b => b.InstanceInfo == e.Instance)?
                    .InstanceStateChanged(e.State);
            }
        }

        protected virtual void InstanceAuthReady(object sender, InstanceAuthReadyEventArgs e)
        {
            lock (Lockher)
            {
                var battle = Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) as DiscoveryBattle;
                battle?.OnInstanceAuthReady(e.CharacterId, e.Auth);
            }
        }

        private void FleetLeaves(object sender, InstanceFleetLeavesEventArgs e)
        {
            lock (Lockher)
            {
                var battle = Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) as DiscoveryBattle;
                battle?.OnFleetLeavesFromInstance(e.FleetType, e.FleetId, e.Hex);
            }
        }

        private void ShipStatusUpdated(object sender, ShipStatusUpdatedEventArgs e)
        {
            lock (Lockher)
            {
                var battle = Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) as DiscoveryBattle;
                var character = battle?.GetCharByShipId(e.ShipId);

                character?.DiscoveryClient?.Invoke(() =>
                {
                    character.UpdateShipStatus(e.ShipId, e.ShipData, e.ShipStats);
                });
            }
        }

        private void AddCharacterShipsXp(object sender, AddCharacterShipsXpEventArgs e)
        {
            if (Server?.GetCharacter(e.CharacterId) is ServerCharacter character)
            {
                character.AddCharacterXp(e.Ships.Sum(s => s.Value));
                character.AddCharacterShipsXp(e.Ships);
            }
        }


        private void OnNewInstanceAction(object sender, InstanceActionEventArgs e)
        {
            switch (e.Action)
            {
                case "enemy_ship_destroyed":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is DiscoveryBattle battle)
                            battle.OnMobDestroyed(e.Data);
                    }
                    break;
                case "update_character_stats":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is DiscoveryBattle battle)
                            battle.UpdateCharacterStats(e.Data);
                    }
                    break;
                default:
                    break;
            }
        }

        public void AddBattle(MatchmakerBattle battle)
        {
            lock (Lockher)
            {
                Battles.Add(battle);
            }
        }


        public string CreateBattleIpAddress()
        {
            if (InstanceManager?.RemoteEndPoint.Address is IPAddress address)
            {
                if (address.IsIPv4MappedToIPv6 == true)
                    address = address.MapToIPv4();

                if (address.Equals(IPAddress.Loopback) == false &&
                    address.Equals(IPAddress.IPv6Loopback) == false)
                    return address.ToString();
            }

            return IPAddress.Any.ToString();
        }
    }
}

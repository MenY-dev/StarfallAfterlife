using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class SfaMatchmaker
    {
        public SfaServer Server { get; set; }

        public Uri InstanceManagerAddress { get; set; }

        public InstanceManagerClient InstanceManager { get; protected set; }

        public DiscoveryGameMode DiscoveryGameMode { get; protected set; }

        public MothershipAssaultGameMode MothershipAssaultGameMode { get; protected set; }

        public QuickMatchGameMode QuickMatchGameMode { get; protected set; }

        public RankedGameMode RankedGameMode { get; protected set; }

        public HashSet<MatchmakerGameMode> GameModes { get; } = new();

        public HashSet<MatchmakerBattle> Battles { get; } = new();

        protected ActionBuffer Actions { get; } = new();

        protected object Lockher { get; } = new();

        public virtual void Start()
        {
            Stop();

            DiscoveryGameMode ??= new DiscoveryGameMode();
            MothershipAssaultGameMode ??= new MothershipAssaultGameMode();
            QuickMatchGameMode ??= new QuickMatchGameMode();
            RankedGameMode ??= new RankedGameMode();
            GameModes.Add(DiscoveryGameMode);
            GameModes.Add(MothershipAssaultGameMode);
            GameModes.Add(QuickMatchGameMode);
            GameModes.Add(RankedGameMode);
            DiscoveryGameMode.Init(this);
            MothershipAssaultGameMode.Init(this);
            QuickMatchGameMode.Init(this);
            RankedGameMode.Init(this);

            InstanceManager = new InstanceManagerClient();
            InstanceManager.CharacterDataRequested += CharacterDataRequested;
            InstanceManager.MobDataRequested += MobDataRequested;
            InstanceManager.SpecialFleetRequested += SpecialFleetRequested;
            InstanceManager.DropListRequested += DropListRequested;
            InstanceManager.RewardForEvenRequested += RewardForEvenRequested;
            InstanceManager.InstanceStateChanged += InstanceStateChanged;
            InstanceManager.InstanceAuthReady += InstanceAuthReady;
            InstanceManager.FleetLeaves += FleetLeaves;
            InstanceManager.ShipStatusUpdated += ShipStatusUpdated;
            InstanceManager.AddCharacterShipsXp += AddCharacterShipsXp;
            InstanceManager.NewInstanceAction += OnNewInstanceAction;
            InstanceManager.RankedFleetRequested += RankedFleetRequested; ;
            InstanceManager.ConnectAsync(InstanceManagerAddress).Wait();

            SfaDebug.Print($"Matchmaker Started! (InstanceManagerAddress = {InstanceManagerAddress})");
        }


        public void Stop()
        {
            InstanceManager?.Close();
        }

        protected virtual void CharacterDataRequested(object sender, CharacterDataRequestEventArgs e)
        {
            if (Server?.GetCharacter(e.CharacterId) is ServerCharacter character)
            {
                var comparison = StringComparison.InvariantCultureIgnoreCase;

                if ("battlegrounds".Equals(e.GameMode, comparison) ||
                    "srv".Equals(e.GameMode, comparison))
                {
                    character.DiscoveryClient?
                        .RequestDiscoveryCharacterData(true)
                        .ContinueWith(t => InstanceManager.SendCharacterData(e.CharacterId, JsonNode.Parse(t.Result?.ToJsonString())));
                }
                else if ("station_attack".Equals(e.GameMode, comparison))
                {
                    character.DiscoveryClient?
                        .RequestDiscoveryCharacterData(false)
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
            JsonNode data = null;

            if (e.InstanceAuth is not null)
            {
                if (GetBattle(e.InstanceAuth) is MatchmakerBattle battle)
                {
                    data = battle.GetMobData(new MobDataRequest
                    {
                        IsCustom = e.IsCustom,
                        MinLvl = e.MinLvl,
                        MaxLvl = e.MaxLvl,
                        Faction = e.Faction,
                        FleetId = e.MobId,
                        Tags = e.Tags?.ToArray() ?? Array.Empty<string>(),
                    });
                }
            }

            InstanceManager.SendMobData(e.InstanceAuth, e.MobId, data ?? new JsonObject());
        }

        private void SpecialFleetRequested(object sender, SpecialFleetRequestEventArgs e)
        {
            var ships = new JsonArray();

            if (e.Auth is not null)
            {
                if (GetBattle(e.Auth) is MatchmakerBattle battle)
                {
                    ships = battle.GetSpecialFleet(e.FleetName) ?? new();
                }
            }

            InstanceManager.SendSpecialFleetData(e.FleetName, e.Auth, ships);
        }


        private void DropListRequested(object sender, DropListRequestEventArgs e)
        {
            JsonArray drop = null;

            if (GetBattle(e.Auth) is MatchmakerBattle battle)
                drop = battle.GetDropList(e.DropName);

            InstanceManager.SendDropList(e.DropName, e.Auth, drop?.ToJsonString() ?? "[]");
        }

        private void RewardForEvenRequested(object sender, RewardForEvenRequestEventArgs e)
        {
            string reward = null;

            if (GetBattle(e.Auth) is MatchmakerBattle battle)
            {
                reward = new JsonObject
                {
                    ["charact_reward_queue"] = JsonHelpers.ParseNodeUnbuffered(battle.GetRewards()),
                }.ToJsonString(false);
            }

            InstanceManager.SendRewardForEven(reward ?? "{}", e.Auth);
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
                var battle = Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance);

                if (battle is DiscoveryBattle discoveryBattle)
                    discoveryBattle?.OnFleetLeavesFromInstance(e.FleetType, e.FleetId, e.Hex);
                else if (battle is StationAttackBattle stationAttackBattle)
                    stationAttackBattle?.OnFleetLeavesFromInstance(e.FleetType, e.FleetId, e.Hex);

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
                character.AddCharacterCurrencies(xp: e.Ships.Sum(s => s.Value), shipsXp: e.Ships);
            }
        }

        private void RankedFleetRequested(object sender, RankedFleetRequestEventArgs e)
        {
            lock (Lockher)
            {
                var battle = Battles.FirstOrDefault(b => b.InstanceInfo?.Auth == e.InstanceAuth) as RankedBattle;
                var fleet = battle?.GetRankedFleet(e.FleetId);
                InstanceManager.SendRankedFleetData(fleet, e.FleetId, e.InstanceAuth);
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
                case "pirates_assault_status":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is DiscoveryBattle battle)
                            battle.OnPiratesAssaultStatusUpdated(e.Data);
                    }
                    break;
                case "save_ships_group":
                    {
                        if (JsonHelpers.ParseNodeUnbuffered(e.Data) is JsonObject data &&
                            Server?.GetCharacter((int?)data["char_id"] ?? -1) is ServerCharacter character)
                        {
                            character.SaveShipsGroup((string)data["group"]);
                        }
                    }
                    break;
                case "object_interact_event":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is DiscoveryBattle battle)
                            battle.HandleInstanceObjectInteractEvent(e.Data);
                    }
                    break;
                case "secret_object_looted":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is DiscoveryBattle battle)
                            battle.HandleSecretObjectLooted(e.Data);
                    }
                    break;
                case "report_ore_taken":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is DiscoveryBattle battle)
                            battle.HandleOreTaken(JsonHelpers.DeserializeUnbuffered<InventoryItem[]>(e.Data) ?? Array.Empty<InventoryItem>()); ;
                    }
                    break;
                case "battle_results":
                    {
                        if (Battles.FirstOrDefault(b => b.InstanceInfo == e.Instance) is var battle)
                            battle.HandleBattleResults(JsonHelpers.ParseNodeUnbuffered(e.Data) ?? "");
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnUserStatusChanged(SfaServerClient user, UserInGameStatus status)
        {
            lock (Lockher)
            {
                if (user is null)
                    return;

                foreach (var battle in GetBattles(user))
                    battle?.UserStatusChanged(user, status);

                foreach (var character in user.DiscoveryClient?.Characters?.ToArray() ?? Array.Empty<ServerCharacter>())
                {
                    foreach (var battle in GetBattles(character))
                        battle?.CharStatusChanged(character, status);
                }
            }
        }

        public void AddBattle(MatchmakerBattle battle)
        {
            lock (Lockher)
            {
                Battles.Add(battle);
            }
        }


        internal void RemoveBattle(MatchmakerBattle battle)
        {
            lock (Lockher)
            {
                Battles.Remove(battle);
            }
        }

        public MatchmakerBattle GetBattle(ServerCharacter character)
        {
            lock (Lockher)
            {
                return GetBattles(character).FirstOrDefault();
            }
        }

        public MatchmakerBattle GetBattle(string auth)
        {
            lock (Lockher)
            {
                if (auth is null)
                    return null;

                return Battles.FirstOrDefault(b => b.InstanceInfo?.Auth == auth);
            }
        }

        public MatchmakerBattle[] GetBattles(ServerCharacter character)
        {
            lock (Lockher)
            {
                return Battles.Where(b => b?.ContainsChar(character) == true).ToArray();
            }
        }

        public MatchmakerBattle[] GetBattles(SfaServerClient user)
        {
            lock (Lockher)
            {
                return Battles.Where(b => b?.ContainsUser(user) == true).ToArray();
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

        public void Invoke(Action action) => Actions.Invoke(action);

        public void Invoke(Action action, TimeSpan delay) => Actions.Invoke(action, delay);
    }
}

using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Quests;
using StarfallAfterlife.Bridge.Events;
using StarfallAfterlife.Bridge.Server.Inventory;
using StarfallAfterlife.Bridge.Instances;
using System.Collections;
using System.Text.Json.Nodes;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
using System.Reflection.Emit;
using static StarfallAfterlife.Bridge.Native.Windows.Win32;
using StarfallAfterlife.Bridge.Houses;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public partial class ServerCharacter
    {
        public DiscoveryClient DiscoveryClient { get; set; }

        public SfaRealm Realm => DiscoveryClient?.Server?.Realm;

        public SfaDatabase Database => Realm?.Database;

        public int Id { get; set; } = -1;

        public Guid Guid { get; set; } = Guid.Empty;

        public Guid ProfileId => DiscoveryClient?.Client?.ProfileId ?? Guid.Empty;

        public string Name { get; set; }

        public int UniqueId { get; set; } = -1;

        public string UniqueName { get; set; }

        public string HouseTag { get; set; }

        public Faction Faction { get; set; }

        public int Level { get; set; }

        public int AccessLevel { get; set; }

        public int Xp { get; set; }

        public float XpFactor { get; set; }

        public int BonusXp { get; set; }

        public int IGC { get; set; }

        public int BGC { get; set; }

        public int CurrentDetachment { get; set; } = 0;

        public List<int> DetachmentSlots { get; set; } = new List<int>();

        public List<ShipConstructionInfo> Ships { get; protected set; } = new();

        public UserFleet Fleet { get; set; }

        public bool IsOnline { get; protected set; }

        public Dictionary<int, SystemHexMap> ExplorationProgress => Progress.Systems;

        public CharacterProgress Progress { get; protected set; } = new();

        public List<QuestListener> ActiveQuests { get; } = new();

        public CharacterInventory Inventory { get; protected set; }

        public MulticastEvent Events { get; protected set; } = new();

        public SelectionInfo Selection { get; set; } = null;

        public List<int> Abilities { get; } = new();

        public List<ShipsGroup> ShipGroups { get; } = new();

        public CharacterParty Party { get; set; }

        protected Dictionary<int, DateTime> AbilitiesCooldown { get; } = new();

        private readonly object _characterLocker = new();

        public void LoadFromCharacterData(JsonNode doc)
        {
            if (doc is null)
                return;

            XpFactor = (float?)doc["c_xp_boost"] ?? 1;
            BonusXp = (int?)doc["bonus_xp"] ?? 0;
            Xp = (int?)doc["xp"] ?? 0;
            Level = (int?)doc["level"] ?? 0;
            IGC = (int?)doc["igc"] ?? 0;
            BGC = (int?)doc["bgc"] ?? 0;
            Faction = (Faction?)(int?)doc["faction"] ?? Faction.None;
            AccessLevel = (int?)doc["access_level"] ?? 0;
            CurrentDetachment = (int?)doc["currentdetachment"] ?? 0;
            Ships = new();
            DetachmentSlots = new();

            List<ShipConstructionInfo> allShips = new();

            Ships.Clear();
            Abilities.Clear();
            ShipGroups.Clear();

            if (doc["ships"]?.AsArraySelf() is JsonArray ships)
            {
                foreach (var ship in ships)
                {
                    if ((string)ship["data"] is string shipDataString)
                    {
                        try
                        {
                            var shipInfo = JsonSerializer.Deserialize<ShipConstructionInfo>(shipDataString);
                            shipInfo.FleetId = Convert.ToInt32(UniqueId);
                            allShips.Add(shipInfo);
                        }
                        catch { }
                    }
                }
            }

            var detachment = doc["detachments"]?.AsArray()?.FirstOrDefault(d => (int?)d["id"] == CurrentDetachment);

            if (detachment["slots"]?.AsObjectSelf() is JsonObject slotsNode &&
                DiscoveryClient?.Server?.Realm?.Database is SfaDatabase database)
            {
                foreach (var slotData in slotsNode)
                {
                    int slotId;
                    int shipId = (int?)slotData.Value ?? -1;

                    if (int.TryParse(slotData.Key, out slotId) == false)
                        slotId = -1;

                    DetachmentSlots.Add(slotId);

                    if (allShips.FirstOrDefault(s => s.Id == shipId) is ShipConstructionInfo ship)
                    {
                        ship.CargoHoldSize = database.GetShipCargo(ship.Hull);
                        ship.Detachment = CurrentDetachment;
                        ship.Slot = slotId;
                        Ships.Add(ship);
                    }
                }

                if (detachment["abilities"]?.AsArraySelf() is JsonArray abilities)
                {
                    foreach (var ability in abilities)
                    {
                        if ((int?)ability?.AsObjectSelf()?.FirstOrDefault().Value is int abilityId &&
                            abilityId > 0)
                            Abilities.Add(abilityId);
                    }
                }
            }

            if (doc["ship_groups"]?.AsArraySelf() is JsonArray shipGroups)
            {
                ShipGroups.AddRange(shipGroups.DeserializeUnbuffered<List<ShipsGroup>>() ?? new());
            }

            Inventory ??= new(this);
            Events?.Broadcast<ICharacterListener>(l => l.OnCurrencyUpdated(this));
        }

        public void LoadActiveShips(JsonNode doc)
        {
            var database = Database ?? SfaDatabase.Instance;
            (Ships ??= new()).Clear();

            if (doc is JsonArray shipsData)
            {
                foreach (var item in shipsData)
                {
                    if (JsonHelpers.DeserializeUnbuffered<ShipConstructionInfo>(item) is ShipConstructionInfo ship)
                    {
                        ship.FleetId = UniqueId;

                        if (ship.Cargo is InventoryStorage cargo)
                        {
                            foreach (var inventory in cargo.ToArray())
                            {
                                if (inventory.IGCPrice == 0 &&
                                    database.GetItem(inventory.Id) is SfaItem blueprint)
                                {
                                    cargo.Remove(inventory, inventory.Count);
                                    cargo.Add(blueprint, inventory.Count, inventory.UniqueData);
                                }
                            }
                        }

                        Ships.Add(ship);
                    }
                }
            }
        }

        public JsonNode ToDiscoveryCharacterData()
        {
            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new InventoryAsCargoJsonConverter());

            return new JsonObject
            {
                ["faction"] = (int)Faction,
                ["charactname"] = UniqueName ?? "",
                ["xp_factor"] = XpFactor,
                ["bonus_xp"] = BonusXp,
                ["access_level"] = AccessLevel,
                ["level"] = Level,
                ["house_tag"] = HouseTag ?? "",
                ["ships_list"] = Ships.Select(s => new JsonObject
                {
                    ["id"] = s.Id,
                    ["data"] = JsonHelpers.ParseNodeUnbuffered(s, jsonOptions)?.ToJsonString()
                }).ToJsonArray(),
                ["ship_groups"] = JsonHelpers.ParseNodeUnbuffered(ShipGroups),
            };
        }

        public void AcceptQuest(int questId, bool checkQuestLimits = false)
        {
            Progress.CompletedQuests ??= new();
            Progress.ActiveQuests ??= new();

            if (Progress.CompletedQuests.Contains(questId) == true ||
                Progress.ActiveQuests.ContainsKey(questId) == true)
                return;

            if (checkQuestLimits == true && CheckQuestLimitsReached() == true)
            {
                DiscoveryClient.Invoke(c => c.SendQuestLimitNotification(questId));
                return;
            }

            var questProgress = new QuestProgress();

            if (QuestListener.Create(questId, this) is QuestListener listener)
            {
                Progress.ActiveQuests[questId] = questProgress;
                ActiveQuests.Add(listener);
                listener.StartListening();
                DiscoveryClient?.SyncAcceptNewQuest(questId, questProgress);
                listener.RaiseInitialActions();
            }
        }

        public bool AcceptDynamicQuest(DiscoveryQuest quest, bool checkQuestLimits = false)
        {
            if (quest is null)
                return false;

            var activeQuests = Progress.ActiveQuests ??= new();
            var questId = Enumerable
                .Range(1, activeQuests.Count + 1)
                .Select(i => new QuestIdInfo
                {
                    LocalId = i,
                    Type = quest.Type,
                    Faction = quest.ObjectFaction,
                    IsDynamicQuest = true,
                }.ToId())
                .FirstOrDefault(i => activeQuests.ContainsKey(i) == false);

            if (checkQuestLimits == true && CheckQuestLimitsReached() == true)
            {
                DiscoveryClient.Invoke(c => c.SendQuestLimitNotification(questId));
                return false;
            }

            var questProgress = new QuestProgress() { QuestData = quest };

            if (QuestListener.Create(quest, this) is QuestListener listener)
            {
                Progress.ActiveQuests[questId] = questProgress;
                ActiveQuests.Add(listener);
                listener.StartListening();
                DiscoveryClient?.SyncAcceptNewQuest(questId, questProgress);
                listener.RaiseInitialActions();
            }

            return true;
        }

        protected bool CheckQuestLimitsReached()
        {
            var countPredicate = (QuestListener q) =>
                q?.Info?.Type is QuestType.Task or QuestType.HouseTask;

            return ActiveQuests.ToArray().Count(countPredicate) >= 30;
        }

        public void AbandoneQuest(int questId)
        {
            if (ActiveQuests.ToArray().FirstOrDefault(q => q?.Id == questId) is QuestListener quest)
            {
                quest.StopListening();
                quest.State = QuestState.Abandoned;
                ActiveQuests.Remove(quest);
                Progress?.ActiveQuests?.Remove(questId);
                DiscoveryClient?.SendQuestCompleteData(quest);
                DiscoveryClient?.SyncQuestCanceled(questId);
            }
        }

        public void UpdateQuestProgress(int questId, QuestProgress questProgress)
        {
            Progress.CompletedQuests ??= new();
            Progress.ActiveQuests ??= new();

            if (Progress.ActiveQuests.ContainsKey(questId) == true &&
                Progress.CompletedQuests.Contains(questId) == false)
            {
                Progress.ActiveQuests[questId] = questProgress;
                DiscoveryClient?.SyncQuestProgress(questId, questProgress);
            }
        }


        public void FinishQuest(int questId)
        {
            Progress.CompletedQuests ??= new();
            Progress.ActiveQuests ??= new();

            if (ActiveQuests.FirstOrDefault(q => q?.Id == questId) is QuestListener quest &&
                quest.CheckCompleteon() == true)
            {
                quest.StopListening();
                quest.State = QuestState.Done;
                ActiveQuests.Remove(quest);
                Progress.ActiveQuests.Remove(questId, out var progress);
                
                if ((progress?.IsDynamic ?? QuestIdInfo.Create(questId).IsDynamicQuest) == false)
                    Progress.CompletedQuests.Add(questId);

                DiscoveryClient?.Invoke(c =>
                {
                    c.SendQuestCompleteData(quest);
                    c.SyncQuestCompleted(questId);
                });

                Events?.Broadcast<ICharacterListener>(l => l.OnQuestCompleted(this, quest));

                if (quest.Info?.Reward is QuestReward reward)
                {
                    var xpFactor = (quest.Info?.Level ?? 0) < AccessLevel ? 0.1 : 1;
                    var igc = reward.IGC > 0 ? reward.IGC : default(int?);
                    var xp = reward.Xp > 0 ? (int)(reward.Xp * xpFactor) : default(int?);
                    var bgc = SfaDatabase.AccessLevelToQuestBGReward(quest.Info?.Level ?? 1);

                    AddCharacterCurrencies(igc: igc, bgc: bgc, xp: xp);


                    if (reward.Items is not null &&
                        (Database ?? SfaDatabase.Instance) is SfaDatabase database)
                    {
                        DiscoveryClient?.Invoke(c =>
                        {
                            var addedItems = new List<InventoryItem>();

                            foreach (var rewardItem in reward.Items)
                            {
                                if (database.GetItem(rewardItem.Id) is SfaItem itemInfo)
                                {
                                    var item = InventoryItem.Create(itemInfo, rewardItem.Count);
                                    (Inventory ?? new(this)).AddItem(item);
                                    addedItems.Add(item);
                                }
                            }

                            if (addedItems.Count > 0)
                                c.SendInventoryNewItems(addedItems);
                        });
                    }
                    var notification = $"+{bgc} BGC";

                    if (igc is not null)
                        notification += $"\r\n+{igc} IGC";

                    if (xp is not null)
                        notification += $"\r\n+{xp} XP";

                    DiscoveryClient?.Invoke(c => c.SendOnScreenNotification(new SfaNotification
                    {
                        Id = "finish_quest" + quest.Id,
                        Header = "QuestComplete",
                        Text = notification,
                        Format = new()
                    }));
                }
            }
        }

        public void CompleteAllQuests()
        {
            lock (_characterLocker)
            {
                foreach (var item in ActiveQuests.ToArray() ?? Array.Empty<QuestListener>())
                    CompleteQuest(item?.Id ?? -1);
            }
        }

        public void CompleteQuest(int questId)
        {
            lock (_characterLocker)
            {
                Progress.ActiveQuests ??= new();

                if (ActiveQuests.FirstOrDefault(q => q?.Id == questId) is QuestListener quest)
                {
                    try
                    {
                        foreach (var condition in quest.Conditions ?? new())
                        {
                            condition.Progress = condition.ProgressRequire;
                        }
                    }
                    catch { }
                }
            }
        }

        public bool CheckReward(int rewardId)
        {
            lock (_characterLocker)
                return Progress.TakenRewards.Contains(rewardId);

        }

        public bool AddReward(int rewardId)
        {
            lock (_characterLocker)
            {
                if (CheckReward(rewardId) == false &&
                    Realm?.CharacterRewardDatabase?.GetReward(rewardId) is CharacterReward reward &&
                    Database.GetItem(reward.RewardId) is SfaItem item &&
                    Progress.TakenRewards.Add(rewardId) == true)
                {
                    DiscoveryClient?.SyncCharReward(rewardId);
                    var dst = CargoTransactionEndPoint.CreateForCharacterInventory(this);
                    dst.Receive(item, reward.Count);
                    return true;
                }

                return false;
            }
        }

        public InventoryStorage GetShipCargoByStockName(string stockName) =>
            GetShipByStockName(stockName)?.Cargo;

        public string CreateShipStocName(int shipId) => "cargo_ship_" + shipId;

        public ShipConstructionInfo GetShipByStockName(string stockName)
        {
            return Ships?.FirstOrDefault(s => stockName == CreateShipStocName(s.Id));
        }

        public ShipConstructionInfo GetShipById(int shipId)
        {
            return Ships?.FirstOrDefault(s => s.Id == shipId);
        }

        public int AddItemToStocks(int id, int count = 0, string uniqueData = null, bool includeInventory = false)
        {
            if (count < 1)
                return 0;

            if (Database?.GetItem(id) is SfaItem item)
            {
                var fleet = CargoTransactionEndPoint.CreateForCharacterFleet(this);
                var result = fleet.Receive(item, count, uniqueData);

                if (includeInventory == true && result < count)
                {
                    var inv = CargoTransactionEndPoint.CreateForCharacterInventory(this);
                    result += inv.Receive(item, count - result);
                }

                RaseStockUpdated();

                DiscoveryClient?.Invoke(c =>
                {
                    c.SendFleetCargo();
                    c.SyncSessionFleetInfo();
                });

                return result;
            }

            return 0;
        }

        public void RaseStockUpdated()
        {
            Events?.Broadcast<IStockListener>(l => l.OnStockUpdated());
        }

        public ShipConstructionInfo GetMainShip()
        {
            return Ships?.LastOrDefault();
        }

        public UserFleet CreateNewFleet()
        {
            Fleet = new UserFleet();
            UpdateFleetInfo();
            return Fleet;
        }

        public void UpdateFleetInfo()
        {
            var mainShip = GetMainShip() ?? new();

            if (Fleet is UserFleet fleet)
            {
                fleet.Id = UniqueId;
                fleet.Name = string.IsNullOrWhiteSpace(HouseTag) ? UniqueName : $"[{HouseTag}] {UniqueName}";
                fleet.Faction = Faction;
                fleet.Level = Level;
                fleet.BaseVision = 5;
                fleet.BaseSpeed = 5;
                fleet.Hull = mainShip.Hull;
                fleet.Skin = mainShip.ShipSkin;
                fleet.SkinColor1 = mainShip.SkinColor1;
                fleet.SkinColor2 = mainShip.SkinColor2;
                fleet.SkinColor3 = mainShip.SkinColor3;
                fleet.Decal = mainShip.ShipDecal;
            }

            foreach (var item in Abilities)
                Ability.ApplyPassiveEffects(Fleet, item);
        }

        public void UpdateShipStatus(int shipId, string shipData = null, string shipStats = null)
        {
            if (Ships?.FirstOrDefault(s => s.Id == shipId) is ShipConstructionInfo ship &&
                JsonHelpers.ParseNodeUnbuffered(shipData) is JsonObject data)
            {

                ship.ArmorDelta = (float?)data["armor"] ?? 0;
                ship.StructureDelta = (float?)data["structure"] ?? 0;

                if ((int?)data["destroyed"] is int isShipdDestroyed &&
                    isShipdDestroyed != 0)
                {
                    Ships.RemoveAll(s => s.Id == shipId);
                    DiscoveryClient?.Invoke(k => k.SyncShipDestroyed(shipId));

                    if (Ships.Count < 1)
                        DiscoveryClient?.Invoke(k => k.FinishGalaxySession());
                }
                else
                {
                    if (data["hplist"]?.DeserializeUnbuffered<List<ShipHardpoint>>() is List<ShipHardpoint> hplist)
                    {
                        ship.HardpointList ??= new();
                        ship.HardpointList.Clear();
                        ship.HardpointList.AddRange(hplist);
                    }

                    (ship.Cargo ??= new()).Clear();

                    if (data["cargo_list"] is JsonArray cargo &&
                        DiscoveryClient?.Server?.Realm?.Database is SfaDatabase database)
                    {
                        foreach (var item in cargo)
                        {
                            if (item is JsonObject &&
                                (int?)item["entity"] is int entity &&
                                (int?)item["count"] is int count &&
                                database.GetItem(entity) is SfaItem itemImfo)
                                ship.Cargo.Add(itemImfo, count, (string)item["unique_data"]);
                            continue;
                        }
                    }
                }

                DiscoveryClient?.SyncSessionFleetInfo();
                DiscoveryClient?.SendFleetCargo();
            }
        }


        public void RepairAndRefuelFleet()
        {
            if (Ships?.ToArray() is ShipConstructionInfo[] fleet)
            {
                foreach (var ship in fleet)
                {
                    if (ship is null)
                        continue;

                    ship.ArmorDelta = 0;
                    ship.StructureDelta = 0;

                    foreach (var item in ship.HardpointList.ToArray()
                        .SelectMany(h => h.EquipmentList.ToArray()))
                    {
                        item.IsDestroyed = 0;
                    }
                }

                DiscoveryClient.Invoke(c => c.SyncSessionFleetInfo());
            }
        }

        public void HandleBattleResults(JsonNode doc)
        {
            if (doc is not JsonObject)
                return;

            var xp = (int?)doc["xp_earned"] ?? 0;
            var ships = doc["ships"]?.AsArraySelf().Select(i => (int?)i?["shipid"] ?? -1).Where(i => i > -1).ToList() ?? new();
            var shipsXp = new Dictionary<int, int>();

            if (ships.Count > 0)
            {
                var oneShipXp = xp / ships.Count;

                foreach (var i in ships)
                    shipsXp[i] = oneShipXp;
            }

            foreach (var item in doc["ships"]?.AsArraySelf() ?? new())
                if ((int?)item["shipid"] is int id)
                    shipsXp[id] = 0;

            AddCharacterCurrencies(
                igc: (int?)doc["igc_earned"],
                bgc: (int?)doc["bgc_earned"],
                xp: xp,
                shipsXp: shipsXp);
        }

        public void ReacallShip(int shipId, int slotId)
        {
            DiscoveryClient?.SendFleetRecallStateUpdate(FleetRecallState.Started, 1, slotId);

            DiscoveryClient?.RequestShipData(shipId).ContinueWith(t =>
            {
                if (t.Result is ShipConstructionInfo ship)
                {
                    ship.Detachment = CurrentDetachment;
                    ship.Slot = slotId;

                    if (DiscoveryClient?.Server?.Realm?.Database is SfaDatabase database)
                        ship.CargoHoldSize = database.GetShipCargo(ship.Hull);

                    Ships.Add(ship);
                    DiscoveryClient?.SyncSessionFleetInfo();
                    DiscoveryClient?.SendFleetCargo();
                    DiscoveryClient?.SendFleetRecallStateUpdate(FleetRecallState.Done, 0, slotId);
                }
                else
                {
                    DiscoveryClient?.SendFleetRecallStateUpdate(FleetRecallState.Failed, 0, slotId);
                }
            });
        }

        public void LoadQuestsFromProgress()
        {
            foreach (var item in ActiveQuests)
            {
                item?.StopListening();
                item.State = QuestState.Abandoned;
                DiscoveryClient.SendQuestCompleteData(item);
            }

            ActiveQuests.Clear();

            foreach (var item in Progress?.ActiveQuests ?? new())
            {
                if (QuestListener.Create(item.Key, this, item.Value) is QuestListener quest)
                {
                    ActiveQuests.Add(quest);
                    quest.StartListening();
                    DiscoveryClient.SendQuestCompleteData(quest);
                }
            }

            DiscoveryClient.SendQuestDataUpdate();
        }

        public void LoadProgress(CharacterProgress progress)
        {
            Progress = progress ?? new();
            Inventory ??= new(this);
            LoadQuestsFromProgress();
        }

        public void AddCharacterCurrencies(
            int? igc = null,
            int? bgc = null,
            int? xp = null,
            Dictionary<int, int> shipsXp = null)
        {
            if (igc is not null)
                IGC = IGC.AddWithoutOverflow((int)igc);

            if (bgc is not null)
                BGC = BGC.AddWithoutOverflow((int)bgc);

            if (xp is not null)
            {
                Xp = Xp.AddWithoutOverflow((int)xp);
                AddXpToSeasons((int)xp);
                DiscoveryClient?.AddXpToHouse((long)xp);
            }

            if (shipsXp is not null)
            {
                foreach (var item in shipsXp)
                {
                    var ship = Ships.FirstOrDefault(s => s?.Id == item.Key);

                    if (ship is not null)
                        ship.Xp = ship.Xp.AddWithoutOverflow(item.Value);
                }
            }

            DiscoveryClient?.SendAddCharacterCurrencies(UniqueId, igc, bgc, xp, shipsXp);
            Events?.Broadcast<ICharacterListener>(l => l.OnCurrencyUpdated(this));
        }


        public List<DiscoveryDropRule> CreateDropRules()
        {
            return ActiveQuests.ToArray()?
                .Select(q => q.CreateDropRules())
                .Where(r => r is not null)
                .SelectMany(r => r)
                .Where(r => r is not null)
                .ToList() ?? new();
        }

        public List<QuestTileParams> CreateInstanceTileParams(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            return ActiveQuests.ToArray()?
                .Select(q => q.CreateInstanceTileParams(systemId, objectType, objectId))
                .Where(r => r is not null)
                .SelectMany(r => r)
                .Where(r => r.TileName is not null)
                .ToList() ?? new();
        }

        public void ProcessSyncCharacterCurrencies(JsonNode doc)
        {
            if (doc is not JsonObject)
                return;

            var previousLevel = Level;

            Xp = (int?)doc["xp"] ?? 0;
            Level = (int?)doc["level"] ?? 0;
            AccessLevel = (int?)doc["access_level"] ?? 0;
            IGC = (int?)doc["igc"] ?? 0;
            BGC = (int?)doc["bgc"] ?? 0;

            if (Level != previousLevel)
                DiscoveryClient?.Invoke(c => c.UpdateHouseMemberInfo());

            Events?.Broadcast<ICharacterListener>(l => l.OnCurrencyUpdated(this));
        }


        public void ProcessSyncCharacterNewResearch(JsonNode doc)
        {
            if (doc is not JsonObject)
                return;

            if (doc["new_items"]?.DeserializeUnbuffered<List<int>>() is List<int> newItems)
            {
                DiscoveryClient?.Invoke(() =>
                {
                    foreach (var item in newItems)
                        Events?.Broadcast<ICharacterListener>(l => l.OnProjectResearch(this, item));
                });
            }
        }

        public Task<bool> CheckResearch(int entityId)
        {
            return DiscoveryClient?.Client?.SendRequest(
                SfaServerAction.RequestItemResearch, 
                new JsonObject { ["entity_id"] = entityId })
                .ContinueWith(t =>
                {
                    if (t.Result is SfaClientResponse response &&
                        response.IsSuccess == true &&
                        JsonHelpers.ParseNodeUnbuffered(response.Text) is JsonObject doc &&
                        (bool?)doc["explored"] == true)
                        return true;

                    return false;
                }) ?? Task.FromResult(false);
        }

        public void AddNewStats(Dictionary<string, float> stats)
        {
            if (stats is null)
                return;

            foreach (var item in stats)
                DiscoveryClient?
                    .Invoke(s => Events?
                    .Broadcast<ICharacterListener>(l => l
                    .OnNewStatsReceived(this, item.Key, item.Value)));

            DiscoveryClient?.Invoke(c => c.AddNewCharacterStats(stats));
        }

        public void UseAbility(int abilityId, int systemId, SystemHex hex)
        {
            lock (_characterLocker)
            {
                var dtb = SfaDatabase.Instance;
                AbilitiesCooldown[abilityId] = DateTime.Now.AddSeconds(dtb.GetAbility(abilityId)?.Cooldown ?? 0);
                Ability.Use(this, abilityId, systemId, hex);
                DiscoveryClient?.SyncAbilities(Fleet);
            }
        }

        public KeyValuePair<int, float>[] GetAbilitiesCooldown()
        {
            var now = DateTime.Now;

            lock (_characterLocker)
                return AbilitiesCooldown.Select(a => KeyValuePair.Create(
                    a.Key, Math.Max(0, (float)(a.Value - now).TotalSeconds))).ToArray();
        }

        public void SaveShipsGroup(string group)
        {
            DiscoveryClient.Invoke(() =>
            {
                var newGroup = JsonHelpers.DeserializeUnbuffered<ShipsGroup>(group ?? "");

                ShipGroups.RemoveAll(g => g.Number == newGroup.Number);
                ShipGroups.Add(newGroup);

                DiscoveryClient?.Client?.Send(new JsonObject
                {
                    ["char_id"] = UniqueId,
                    ["group"] = group
                }, SfaServerAction.SaveShipsGroup);
            });
        }

        public void HandleInstanceObjectInteractEvent(int systemId, string eventType, string eventData)
        {
            DiscoveryClient.Invoke(() =>
            {
                Events.Broadcast<ICharacterInstanceListener>(l =>
                    l.OnInstanceInteractEvent(this, systemId, eventType, eventData));
            });
        }

        public void HandleSecretObjectLooted(int secretId)
        {
            DiscoveryClient.Invoke(() =>
            {
                Progress?.AddSecretObject(secretId);
                DiscoveryClient.SyncSecretObject(secretId);
            });
        }

        public void SetOnlineStatus(bool isOnline)
        {
            if (isOnline != IsOnline)
            {
                IsOnline = isOnline;
                DiscoveryClient?.Invoke(c => c.BroadcastHouseCharacterOnlineStatus(isOnline));
            }
        }
    }
}

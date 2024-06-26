﻿using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class Character
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("guid")]
        public Guid Guid { get; set; } = Guid.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = "NewCharacter";

        [JsonPropertyName("faction")]
        public int Faction { get; set; } = 0;

        [JsonPropertyName("has_premium")]
        public int HasPremium { get; set; } = 0;

        [JsonPropertyName("xp_boost")]
        public double XpBoost => GetEffect(1160329638) > 0 ? 1.5 : 1;

        [JsonPropertyName("igc_boost")]
        public double IgcBoost => GetEffect(503112805) > 0 ? 2 : 1;

        [JsonPropertyName("craft_boost")]
        public double CraftBoost => GetEffect(1464674507) > 0 ? 0.7 : 1;

        [JsonPropertyName("premium_minutes_left")]
        public int PremiumMinutesLeft { get; set; } = 0;

        [JsonPropertyName("xp_minutes_left")]
        public double XpMinutesLeft => GetEffect(1160329638);

        [JsonPropertyName("igc_minutes_left")]
        public double IgcMinutesLeft => GetEffect(503112805);

        [JsonPropertyName("craft_minutes_left")]
        public double CraftMinutesLeft => GetEffect(1464674507);

        [JsonPropertyName("igc")]
        public int IGC { get; set; } = 10000;

        [JsonPropertyName("currentdetachment")]
        public int CurrentDetachment { get; set; } = 1732028966;

        [JsonPropertyName("rank")]
        public int Rank { get; set; } = 0;

        [JsonPropertyName("level")]
        public int Level { get; set; } = 0;

        [JsonPropertyName("access_level")]
        public int AccessLevel { get; set; } = 1;

        [JsonPropertyName("ability_cells")]
        public int AbilityCells { get; set; } = 0;

        [JsonPropertyName("reputation")]
        public int Reputation { get; set; } = 1;

        [JsonPropertyName("xp")]
        public int Xp { get; set; } = 0;

        [JsonPropertyName("ship_slots")]
        public int ShipSlots { get; set; } = 999;

        [JsonPropertyName("selfservice")]
        public string SelfService { get; set; } = "";

        [JsonPropertyName("char_for_tutorial")]
        public int CharForTutorial { get; set; } = 0;

        [JsonPropertyName("production_points")]
        public int ProductionPoints { get; set; } = 3000;

        [JsonPropertyName("production_income")]
        public int ProductionIncome { get; set; } = 12;

        [JsonPropertyName("last_production_income_time")]
        public DateTime LastProductionIncomeTime { get; set; }

        [JsonPropertyName("production_cap")]
        public int ProductionCap { get; set; } = 3000;

        [JsonPropertyName("bgc")]
        public int BGC { get; set; } = 100;

        [JsonPropertyName("bonus_xp")]
        public int BonusXp { get; set; } = 0;

        [JsonPropertyName("end_session_time")]
        public int EndSessionTime { get; set; } = 999999999;

        [JsonPropertyName("bonus_xp_income_minute_elapsed")]
        public int BonusXpIncomeMinuteElapsed { get; set; } = 0;

        [JsonPropertyName("last_ships_repair_time")]
        public DateTime LastShipsRepairTime { get; set; }

        [JsonPropertyName("indiscoverybattle")]
        public int InDiscoveryBattle { get; set; } = 0;

        [JsonPropertyName("ships")]
        public List<FleetShipInfo> Ships { get; set; } = new();

        [JsonPropertyName("detachments")]
        public CharacterDetachments Detachments { get; set; } = new();

        [JsonPropertyName("inventory")]
        public InventoryStorage Inventory { get; set; } = new();

        [JsonPropertyName("project_research")]
        public List<ResearchInfo> ProjectResearch { get; set; } = new();

        [JsonPropertyName("crafting")]
        public List<CraftingInfo> Crafting { get; set; } = new();

        [JsonPropertyName("ship_groups")]
        public List<ShipsGroup> ShipGroups { get; set; } = new();

        [JsonPropertyName("statistic")]
        public Dictionary<string, double> Statistic { get; set; } = new();

        [JsonPropertyName("effects")]
        public CharacterEffectsCollection Effects { get; set; } = new();

        [JsonPropertyName("events")]
        public HashSet<string> Events { get; set; } = new()
        {
            "ShipyardBanDropSessionTutorial"
        };

        [JsonPropertyName("has_session_results")]
        public bool HasSessionResults { get; set; } = false;

        [JsonPropertyName("last_session")]
        [JsonConverter(typeof(SfaObjectJsonConverter<DiscoverySession>))]
        public DiscoverySession LastSession { get; set; }

        [JsonIgnore]
        public int UniqueId { get; set; } = -1;

        [JsonIgnore]
        public SfaDatabase Database { get; set; }

        [JsonIgnore]
        public int CurrentId => UniqueId < 0 ? Id : UniqueId;

        [JsonIgnore]
        public string UniqueName { get; set; } = null;

        [JsonIgnore]
        public string CurrentName => UniqueName is null ? Name : UniqueName;

        [JsonIgnore]
        public int IndexSpace { get; set; } = 0;

        [JsonIgnore]
        public bool HasActiveSession { get; set; } = false;

        [JsonIgnore]
        public bool IsReadyToDropSession { get; set; } = false;

        [JsonIgnore]
        public JsonNode ActiveShips { get; set; }
    }
}
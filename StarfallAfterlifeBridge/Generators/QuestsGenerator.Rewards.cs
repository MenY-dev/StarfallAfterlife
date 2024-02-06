using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public partial class QuestsGenerator
    {
        protected Dictionary<int, List<SfaItem>> ItemsForTaskBoardQuest;

        private Dictionary<int, List<SfaItem>> FindItemsForTaskBoardQuest()
        {
            var levels = new Dictionary<int, List<SfaItem>>()
            {
                { 0, new() },
                { 1, new() },
                { 2, new() },
                { 3, new() },
                { 4, new() },
                { 5, new() },
                { 6, new() },
                { 7, new() },
            };

            var database = Realm.Database ?? SfaDatabase.Instance;

            if (database is null)
                return levels;

            var items = database.Equipments.Values
                .Where(e => e.IsImproved == false && CheckItem(e))
                .Cast<SfaItem>()
                .Concat(database.DiscoveryItems.Values.Where(i => CheckItem(database.GetItem(i.ProductItem))))
                .ToList();

            bool CheckItem(SfaItem item) =>
                item is not null &&
                item.IsDefective == false &&
                item.Faction is Faction.None or Faction.Other &&
                item.IsAvailableForTrading;

            foreach (var lvl in levels)
            {
                foreach (var item in items)
                {
                    var minLvl = item.MinLvl;
                    var maxLvl = item.MaxLvl;

                    if (item is DiscoveryItem discoveryItem &&
                        database.GetItem(discoveryItem.ProductItem) is SfaItem product)
                    {
                        minLvl = Math.Max(minLvl, product.MinLvl);
                        maxLvl = Math.Max(maxLvl, product.MaxLvl);
                    }

                    if (lvl.Key >= minLvl &&
                        lvl.Key <= maxLvl)
                        lvl.Value.Add(item);
                }
            }

            return levels;
        }

        private QuestReward GenerateRewardForTaskBoardQuest(DiscoveryQuest quest)
        {
            var database = Realm.Database ?? SfaDatabase.Instance;

            if (database is null || quest is null)
                return default;

            var revard = new QuestReward();
            var rnd = new Random128(quest.Id + quest.ObjectId);

            switch (rnd.Next() % 10)
            {
                case 0:
                    GenerateItems();
                    GenerateIGC();
                    break;

                case < 4:
                    GenerateItems();
                    break;
                default:
                    GenerateIGC();
                    break;
            }

            void GenerateItems()
            {
                ItemsForTaskBoardQuest ??= FindItemsForTaskBoardQuest();

                if (ItemsForTaskBoardQuest.TryGetValue(quest.Level, out var items) == true)
                {
                    var item = items[rnd.Next(0, items.Count)];

                    (revard.Items ??= new()).Add(new QuestRevardItemInfo
                    {
                        Id = item.Id,
                        Count = (rnd.Next() % 10) switch
                        {
                            0 => 4,
                            < 4 => 2,
                            _ => 1
                        }
                    });
                }
            }

            void GenerateIGC()
            {
                int GetConditionCost(JsonNode condition) => (QuestConditionType?)(byte?)condition["type"] switch
                {
                    QuestConditionType.DeliverItems => 100,
                    QuestConditionType.ExploreSystem => 400,
                    QuestConditionType.KillShip => 500,
                    QuestConditionType.KillUniquePirateShip => 2000,
                    QuestConditionType.InterceptShip => 2000,
                    QuestConditionType.ScanUnknownPlanet => 1000,
                    QuestConditionType.CompleteTask => 2000,
                    QuestConditionType.CompleteSpecialInstance => 2000,
                    QuestConditionType.ExploreObject => 1000,
                    QuestConditionType.StatTracking => 10,
                    QuestConditionType.Client => 1000,
                    QuestConditionType.ExploreRelictShip => 1000,
                    QuestConditionType.DeliverMobDrop => 200,
                    QuestConditionType.KillUniquePersonalPirateShip => 2000,
                    QuestConditionType.KillMobShip => 300,
                    QuestConditionType.KillFactionGroupShip => 300,
                    QuestConditionType.KillPiratesOutpost => 1000,
                    QuestConditionType.KillPiratesStation => 5000,
                    QuestConditionType.KillBoss => 2000,
                    QuestConditionType.DeliverQuestItem => 1500,
                    QuestConditionType.ScanSystemObject => 500,
                    QuestConditionType.KillBossOfAnyFactiont => 2000,
                    QuestConditionType.ResearchProject => 5000,
                    QuestConditionType.ReachCharacterLevel => 5000,
                    QuestConditionType.InstanceEvent => 5000,
                    QuestConditionType.PickUpAndDeliverQuestItem => 2000,
                    QuestConditionType.MineQuestItem => 500,
                    QuestConditionType.DeliverRandomItems => 100,
                    QuestConditionType.InterceptPersonalMob => 2000,
                    _ => 0,
                };

                var igc = quest.Conditions.Sum(q => GetConditionCost(q) * ((int?)q["progress_require"] ?? 1));
                revard.IGC = igc * quest.Level;
            }

            int GetConditionXp(JsonNode condition) => (QuestConditionType?)(byte?)condition["type"] switch
            {
                QuestConditionType.DeliverItems => 500,
                QuestConditionType.ExploreSystem => 1000,
                QuestConditionType.KillShip => 1000,
                QuestConditionType.KillUniquePirateShip => 10000,
                QuestConditionType.InterceptShip => 10000,
                QuestConditionType.ScanUnknownPlanet => 5000,
                QuestConditionType.CompleteTask => 5000,
                QuestConditionType.CompleteSpecialInstance => 5000,
                QuestConditionType.ExploreObject => 2000,
                QuestConditionType.StatTracking => 50,
                QuestConditionType.Client => 5000,
                QuestConditionType.ExploreRelictShip => 10000,
                QuestConditionType.DeliverMobDrop => 1000,
                QuestConditionType.KillUniquePersonalPirateShip => 10000,
                QuestConditionType.KillMobShip => 500,
                QuestConditionType.KillFactionGroupShip => 500,
                QuestConditionType.KillPiratesOutpost => 10000,
                QuestConditionType.KillPiratesStation => 25000,
                QuestConditionType.KillBoss => 10000,
                QuestConditionType.DeliverQuestItem => 5000,
                QuestConditionType.ScanSystemObject => 1000,
                QuestConditionType.KillBossOfAnyFactiont => 10000,
                QuestConditionType.ResearchProject => 10000,
                QuestConditionType.ReachCharacterLevel => 10000,
                QuestConditionType.InstanceEvent => 10000,
                QuestConditionType.PickUpAndDeliverQuestItem => 1000,
                QuestConditionType.MineQuestItem => 1000,
                QuestConditionType.DeliverRandomItems => 500,
                QuestConditionType.InterceptPersonalMob => 10000,
                _ => 0,
            };

            var xp = quest.Conditions.Sum(q => GetConditionXp(q) * ((int?)q["progress_require"] ?? 1));
            revard.Xp = xp * quest.Level;

            return revard;
        }
    }
}

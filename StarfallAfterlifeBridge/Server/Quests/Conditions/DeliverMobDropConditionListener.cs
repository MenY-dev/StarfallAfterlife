using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class DeliverMobDropConditionListener : QuestConditionListener
    {
        public int ItemToDeliver { get; set; }

        public string ItemUniqueData { get; set; }

        public float DropChance { get; set; }

        public List<int> Mobs { get; set; }

        public DeliverMobDropConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);

            ItemToDeliver = (int?)doc?["item_to_deliver"] ?? -1;
            ItemUniqueData = (string)doc?["item_to_deliver_unique_data"];
            Mobs = doc?["target_mobs"]?.DeserializeUnbuffered<List<int>>() ?? new();
            DropChance = (float?)doc?["drop_chance"] ?? 1;
        }

        public override void DeliverQuestItems()
        {
            base.DeliverQuestItems();

            if (ProgressRequire > 0 &&
                Progress < ProgressRequire &&
                Quest?.Character is ServerCharacter character)
            {
                var remainingItems = ProgressRequire - Progress;
                var deliveredItems = CargoTransaction.RemoveItemFromFleet(character, ItemToDeliver, remainingItems, ItemUniqueData);

                if (deliveredItems < remainingItems)
                {
                    deliveredItems += character.Inventory?.RemoveItem(ItemToDeliver, remainingItems - deliveredItems, ItemUniqueData).Result ?? 0;
                }

                Progress += Math.Min(deliveredItems, remainingItems);
            }
        }

        public override List<DiscoveryDropRule> CreateDropRules()
        {
            var rules = base.CreateDropRules() ?? new();

            rules.Add(new()
            {
                Type = DropRuleType.Default,
                Chance = DropChance,
                DropPersonalItems = 1,
                Mobs = Mobs?.ToList() ?? new(),
                Items = new() { new()
                {
                    Count = Math.Max(1, ProgressRequire / 20),
                    Id = ItemToDeliver,
                    UniqueData = ItemUniqueData,
                }},
            });

            return rules;
        }
    }
}

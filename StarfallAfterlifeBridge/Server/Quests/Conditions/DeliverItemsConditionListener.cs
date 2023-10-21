using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class DeliverItemsConditionListener : QuestConditionListener, IStockListener
    {
        public int ItemToDeliver { get; protected set; }

        public string ItemUniqueData { get; protected set; }

        public DeliverItemsConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            ItemToDeliver = (int?)doc?["item_to_deliver"] ?? -1;
            ItemUniqueData = (string)doc?["item_to_deliver_unique_data"];
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

        void IStockListener.OnStockUpdated()
        {
            Update();
        }

        public override List<DiscoveryQuestBinding> CreateBindings()
        {
            return Quest?.Info is DiscoveryQuest quest ? new()
            {
                new()
                {
                    ObjectId = quest.ObjectId,
                    ObjectType = (DiscoveryObjectType)quest.ObjectType,
                    SystemId = quest.ObjectSystem,
                    CanBeFinished = true,
                }
            } : null;
        }
    }
}

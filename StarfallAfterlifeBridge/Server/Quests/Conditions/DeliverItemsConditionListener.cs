using StarfallAfterlife.Bridge.Server.Characters;
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

        public DeliverItemsConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            ItemToDeliver = (int?)doc?["item_to_deliver"] ?? -1;
        }

        public override void DeliverQuestItems()
        {
            base.DeliverQuestItems();

            if (ProgressRequire > 0 &&
                Progress < ProgressRequire &&
                Quest?.Character is ServerCharacter character)
            {
                var remainingItems = ProgressRequire - Progress;
                var deliveredItems = CargoTransaction.RemoveItemFromFleet(character, ItemToDeliver, remainingItems);

                if (deliveredItems < remainingItems)
                {
                    deliveredItems += character.Inventory?.RemoveItem(ItemToDeliver, remainingItems - deliveredItems).Result ?? 0;
                }

                Progress += Math.Min(deliveredItems, remainingItems);
            }
        }

        void IStockListener.OnStockUpdated()
        {
            Update();
        }
    }
}

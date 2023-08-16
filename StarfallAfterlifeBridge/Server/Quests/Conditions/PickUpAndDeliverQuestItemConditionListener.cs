using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class PickUpAndDeliverQuestItemConditionListener : DeliverRandomItemsConditionListener
    {
        public int ObjectId { get; set; }

        public byte ObjectType { get; set; }

        public int ObjectSystemId { get; set; }

        public bool ItemTaken { get; set; }

        public PickUpAndDeliverQuestItemConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            ObjectId = (int?)doc?["storage_obj_id"] ?? -1;
            ObjectType = (byte?)doc?["storage_obj_type"] ?? 0;
            ObjectSystemId = (int?)doc?["storage_sys_id"] ?? -1;
        }

        public override QuestLocationInfo GetLocationInfo()
        {
            if (ItemTaken == true)
                return null;

            return new()
            {
                Type = "pick_up_quest_item",
                QuestId = Quest?.Id ?? -1,
                ConditionId = Identity,
                ObjectId = ObjectId,
                ObjectType = (DiscoveryObjectType)ObjectType,
                SystemId = ObjectSystemId,
            };
        }

        public override ConditionProgress SaveProgress()
        {
            var doc = base.SaveProgress() ?? new();
            doc.SetOption("item_taken", ItemTaken ? 1 : 0);
            return doc;
        }

        public override void LoadProgress(ConditionProgress progressInfo)
        {
            base.LoadProgress(progressInfo);
            ItemTaken = progressInfo?.GetOption("item_taken") > 0;
        }

        public override void RaiseAction(string data)
        {
            if (Quest?.Character?.AddItemToStocks(ItemToDeliver, ProgressRequire, true) == ProgressRequire)
            {
                ItemTaken = true;
                RaiseProgressChanged();
            }
        }
    }
}

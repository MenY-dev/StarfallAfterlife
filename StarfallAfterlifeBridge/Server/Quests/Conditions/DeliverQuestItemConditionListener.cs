using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class DeliverQuestItemConditionListener : DeliverRandomItemsConditionListener
    {
        public int ObjectId { get; set; }

        public byte ObjectType { get; set; }

        public int ObjectSystemId { get; set; }

        public DeliverQuestItemConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            ObjectId = (int?)doc?["dest_obj_id"] ?? -1;
            ObjectType = (byte?)doc?["dest_obj_type"] ?? 0;
            ObjectSystemId = (int?)doc?["dest_sys_id"] ?? -1;
        }

        public override List<DiscoveryQuestBinding> CreateBindings()
        {
            return new()
            {
                new()
                {
                    ObjectId = ObjectId,
                    ObjectType = (DiscoveryObjectType)ObjectType,
                    SystemId = ObjectSystemId,
                    CanBeFinished = true,
                }
            };
        }

        public override void RaiseInitialActions()
        {
            base.RaiseInitialActions();
            Quest?.Character?.AddItemToStocks(ItemToDeliver, ProgressRequire, ItemUniqueData, true);
        }
    }
}

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
    public class DeliverRandomItemsConditionListener : DeliverItemsConditionListener
    {
        public DeliverRandomItemsConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }
    }
}

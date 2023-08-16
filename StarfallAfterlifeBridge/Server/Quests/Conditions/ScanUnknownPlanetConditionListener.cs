using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class ScanUnknownPlanetConditionListener : QuestConditionListener, IExplorationListener
    {
        public int? SystemId { get; set; }
        public int ObjectId { get; set; }

        public ScanUnknownPlanetConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {

        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            SystemId = (int?)doc?["target_system"] ?? -1;
            ObjectId = (int?)doc?["target_object_id"] ?? -1;
        }

        void IExplorationListener.OnObjectScanned(StarSystemObject systemObject)
        {
            if (systemObject is null ||
                systemObject.System?.Id != SystemId ||
                systemObject.Type != DiscoveryObjectType.Planet ||
                systemObject.Id != ObjectId)
                return;

            Progress = ProgressRequire;
        }

        void IExplorationListener.OnSystemExplored(int systemId)
        {

        }
    }
}

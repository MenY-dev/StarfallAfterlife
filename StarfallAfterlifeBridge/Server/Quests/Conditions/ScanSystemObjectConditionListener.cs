using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    public class ScanSystemObjectConditionListener : QuestConditionListener, IExplorationListener
    {
        public int SystemId { get; set; }

        public int ObjectId { get; set; }

        public DiscoveryObjectType ObjectType { get; set; }

        public List<int> PlanetTypes { get; set; }

        public ScanSystemObjectConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            SystemId = (int?)doc?["target_system"] ?? -1;
            ObjectId = (int?)doc?["target_object_id"] ?? -1;
            ObjectType = (DiscoveryObjectType?)(byte?)doc?["target_object_type"] ?? DiscoveryObjectType.None;
            PlanetTypes = doc?["planet_types"]?.DeserializeUnbuffered<List<int>>() ?? new();
        }

        void IExplorationListener.OnObjectScanned(StarSystemObject systemObject)
        {
            if (systemObject is null ||
                systemObject.System?.Id != SystemId ||
                systemObject.Type != ObjectType ||
                systemObject.Id != ObjectId)
                return;

            if (systemObject.Type == DiscoveryObjectType.Planet &&
                systemObject is Planet planet &&
                PlanetTypes is not null &&
                PlanetTypes.Count > 0 &&
                PlanetTypes.Contains((int)planet.PlanetType) == false)
                return;

            Progress = ProgressRequire;
        }

        void IExplorationListener.OnSystemExplored(int systemId)
        {

        }
    }
}

using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections;
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

        public bool NeedSpecificObject { get; set; }

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
            NeedSpecificObject = (bool?)doc?["need_specific_object"] ?? true;

            if (Quest?.Info?.LogicName?.ToLowerInvariant().Equals("new_home") == true)
            {
                NeedSpecificObject = false;
                ObjectType = DiscoveryObjectType.Planet;

                if (PlanetTypes.Count < 1)
                    PlanetTypes = new List<int> { 3, 4 };
            }
        }

        void IExplorationListener.OnObjectScanned(StarSystemObject systemObject)
        {
            if (NeedSpecificObject == true)
            {
                if (systemObject is null ||
                    systemObject.System?.Id != SystemId ||
                    systemObject.Type != ObjectType ||
                    systemObject.Id != ObjectId)
                {
                    return;
                }
            }
            else if (systemObject.Type != ObjectType)
            {
                return;
            }
            else if (systemObject is Planet planet &&
                     PlanetTypes is not null &&
                     PlanetTypes.Count > 0 &&
                     PlanetTypes.Contains((int)planet.PlanetType) == false)
            {
                return;
            }

            Progress = ProgressRequire;
        }

        void IExplorationListener.OnSystemExplored(int systemId)
        {

        }

        public void OnObjectExplored(int systemId, DiscoveryObjectType objectType, int objectId)
        {

        }
    }
}

using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
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
    public class ExploreSystemObjectConditionListener : QuestConditionListener, IExplorationListener
    {
        public int SystemLevel { get; set; } = -1;

        public DiscoveryObjectType ObjectType { get; set; } = DiscoveryObjectType.None;


        public ExploreSystemObjectConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            SystemLevel = (int?)doc?["SystemLevel"] ?? -1;
            ObjectType = (DiscoveryObjectType?)(int?)doc?["ObjectType"] ?? DiscoveryObjectType.None;
        }

        public override void Update()
        {
            base.Update();

            if (Quest is QuestListener quest &&
                quest.Realm?.GalaxyMap is GalaxyMap map &&
                quest.Character?.Progress is CharacterProgress progress)
            {
                if (progress.GetObjects(ObjectType).Any(i =>
                    map.GetSystem((GalaxyMapObjectType)ObjectType, i)?.Level == SystemLevel))
                    Progress = ProgressRequire;
            }
        }

        public override void StartListening()
        {
            base.StartListening();
            Update();
        }

        void IExplorationListener.OnObjectScanned(StarSystemObject systemObject)
        {

        }

        void IExplorationListener.OnSystemExplored(int systemId)
        {

        }

        public void OnObjectExplored(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            if (objectType == ObjectType &&
                Quest?.Realm?.GalaxyMap?.GetSystem(systemId) is GalaxyMapStarSystem system &&
                system.Level == SystemLevel)
                Progress = ProgressRequire;
        }
    }
}

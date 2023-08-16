using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Characters;
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
    public class ExploreSystemConditionListener : QuestConditionListener, IExplorationListener
    {
        public int SystemId { get; set; } = -1;

        public ExploreSystemConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            SystemId = (int?)doc?["system_to_explore"] ?? -1;
        }

        public override void Update()
        {
            base.Update();

            if (Quest?.Character?.ExplorationProgress.TryGetValue(SystemId, out var map) == true)
            {
                if (map.Filling > (SystemHexMap.HexesCount * 0.98f))
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
            Update();
        }
    }
}

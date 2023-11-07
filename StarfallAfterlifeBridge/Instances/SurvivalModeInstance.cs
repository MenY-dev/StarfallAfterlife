using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class SurvivalModeInstance : DiscoveryBattleInstance
    {
        public override string Map => DungeonMap;

        protected string DungeonMap { get; set; } = "srv1_normal";

        public override void Init(InstanceManagerServerClient context)
        {
            var extra = new InstanceExtraData();
            extra.LoadFromJsonString(Info?.ExtraData ?? "");

            DungeonMap = extra.SpecOpsDifficulty switch
            {
                0 => "srv1_easy",
                2 => "srv1_hard",
                _ => "srv1_normal",
            };

            base.Init(context);
        }

        public override JsonNode CreateInstanceConfig()
        {
            var doc = base.CreateInstanceConfig();
            doc["game_mode"] = "srv";
            return doc;
        }
    }
}

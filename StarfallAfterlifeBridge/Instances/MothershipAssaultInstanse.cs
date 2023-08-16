using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class MothershipAssaultInstanse : DiscoveryBattleInstance
    {
        public override string Map => DungeonMap;

        protected string DungeonMap { get; set; } = "bg_MothershipAssault";

        public override void Init(InstanceManagerServerClient context)
        {
            if (string.IsNullOrWhiteSpace(Info?.Map) == false)
            {
                DungeonMap = Info.Map;
            }

            base.Init(context);
        }


        public override JsonNode CreateInstanceConfig()
        {
            var doc = base.CreateInstanceConfig();

            doc["game_mode"] = "battlegrounds";

            if (Info.MothershipIncomeOverride > -1)
                doc["mothership_income_override"] = Info.MothershipIncomeOverride;

            return doc;
        }
    }
}

using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

            if (Info is InstanceInfo info)
            {
                float fSpawnTime = info.FreighterSpawnPeriod < 0 ? 90 : info.FreighterSpawnPeriod;
                float snSpawnTime = info.ShieldNeutralizerSpawnPeriod < 0 ? 90 : info.ShieldNeutralizerSpawnPeriod;

                Process?.MapArguments.AddRange(new string[]
                {
                    $"FSpawnTime={Math.Max(5, fSpawnTime)}",
                    $"SNSpawnTime={Math.Max(5, snSpawnTime)}",
                });
            }
        }

        public override JsonNode CreateInstanceConfig()
        {
            var doc = base.CreateInstanceConfig();
            var income = Info.MothershipIncomeOverride;

            doc["game_mode"] = "battlegrounds";

            if (Info.MothershipIncomeOverride > -1)
                doc["mothership_income_override"] = income < 0 ? 80 : income;

            return doc;
        }
    }
}

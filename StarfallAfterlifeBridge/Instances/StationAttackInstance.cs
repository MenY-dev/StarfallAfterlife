using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    internal class StationAttackInstance : DiscoveryBattleInstance
    {
        public override string Map => "spec_ops_station_attack";

        public override JsonNode CreateInstanceConfig()
        {
            var doc = base.CreateInstanceConfig();
            doc["game_mode"] = "station_attack";
            return doc;
        }
    }
}

using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class DiscoveryDungeonInstance : DiscoveryBattleInstance
    {
        public override string Map => DungeonMap;

        protected string DungeonMap { get; set; } = "psa_PiratesStationAttackMap";

        public override void Init(InstanceManagerServerClient context)
        {
            if (Info is InstanceInfo info)
            {
                if (Info?.DungeonType == DungeonType.Station)
                {
                    DungeonMap = info.DungeonFaction switch
                    {
                        Faction.Nebulords => "psa_NebulordsStationDungeon",
                        Faction.Pyramid => "psa_PyramidStationDungeon",
                        _ => "psa_PiratesStationAttackMap",
                    };
                }
                else if (Info?.DungeonType == DungeonType.Outpost)
                {
                    DungeonMap = info.DungeonFaction switch
                    {
                        Faction.Nebulords => "pso_NebulordsOutpostAttack",
                        Faction.Pyramid => "pso_PyramidOutpostAttack",
                        _ => "pso_ScreechersOutpostDungeon",
                    };
                }
            }

            base.Init(context);
        }
    }
}

using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyCircle
    {
        public int Index { get; }

        public Dictionary<int, GalaxyMapStarSystem> Systems { get; } = new();

        public Dictionary<int, GalaxyFactionGroup> Groups { get; } = new();

        public GalaxyMapStarSystem DeprivedStartSystem { get; set; }

        public GalaxyMapStarSystem EclipseStartSystem { get; set; }

        public GalaxyMapStarSystem VanguardStartSystem { get; set; }

        public float MinRadius { get; set; } = 0;

        public float MaxRadius { get; set; } = 0;

        public GalaxyCircle(int index)
        {
            Index = index;
        }

        public void AddSystem(GalaxyMapStarSystem system)
        {
            if (Systems.TryAdd(system.Id, system) == true)
            {
                if (system.FactionGroup < 0)
                    return;

                if (Groups.TryGetValue(system.FactionGroup, out GalaxyFactionGroup group) == true)
                {
                    if (group.Systems.Contains(system) == false)
                        group.Systems.Add(system);
                }
                else
                {
                    Groups.Add(
                        system.FactionGroup,
                        new GalaxyFactionGroup(system.FactionGroup, system));
                }
            }
        }

        public GalaxyMapStarSystem GetStartSystem(Faction faction) => faction switch
        {
            Faction.Deprived => DeprivedStartSystem,
            Faction.Eclipse => EclipseStartSystem,
            Faction.Vanguard => VanguardStartSystem,
            _ => null
        };
    }
}
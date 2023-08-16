using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyFactionGroup
    {
        public int Id { get; }

        public List<GalaxyMapStarSystem> Systems { get; } = new();

        public GalaxyFactionGroup(int id, params GalaxyMapStarSystem[] systems)
        {
            Id = id;
            Systems.AddRange(systems);
        }
    }
}

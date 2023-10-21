using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class MinerMothership : DockableObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.MinerMothership;

        public int Level { get; set; } = 0;

        public MinerMothership(GalaxyMapMinerMotherships motherships, StarSystem system)
        {
            System = system;
            Hex = motherships.Hex;
            Id = motherships.Id;
            Level = system?.Info?.Level ?? 0;
        }
    }
}

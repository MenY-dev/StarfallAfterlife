using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class StarSystemRichAsteroid : StarSystemObject
    {
        public override DiscoveryObjectType Type => DiscoveryObjectType.RichAsteroids;

        public List<int> Ores { get; } = new();

        public StarSystemRichAsteroid(GalaxyMapRichAsteroid mapAsteroid, StarSystem system)
        {
            System = system;
            Hex = mapAsteroid.Hex;
            Id = mapAsteroid.Id;
            Ores.AddRange(mapAsteroid.Ores ?? new());
        }
    }
}

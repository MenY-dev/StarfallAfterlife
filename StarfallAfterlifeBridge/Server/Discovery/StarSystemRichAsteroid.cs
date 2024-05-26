using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Primitives;
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

        public Dictionary<int,int> Ores { get; protected set; } = new();

        public Dictionary<int,int> CurrentOres { get; protected set; } = new();

        public DateTime LastTake { get; protected set; }

        public bool IsPatrial { get; protected set; }

        private int _variantMinOreCount = _minOreCount;
        private int _variantMaxOreCount = _maxOreCount;
        private const int _minOreCount = 210;
        private const int _maxOreCount = 510;

        public StarSystemRichAsteroid(GalaxyMapRichAsteroid mapAsteroid, StarSystem system)
        {
            System = system;
            Hex = mapAsteroid.Hex;
            Id = mapAsteroid.Id;

            var rnd = new Random128(Id);
            var variantsCount = mapAsteroid.Ores.Count;
            _variantMinOreCount = (_minOreCount / variantsCount);
            _variantMaxOreCount = (_maxOreCount / variantsCount);

            if (mapAsteroid is not null)
            {
                foreach (int ore in mapAsteroid.Ores)
                {
                    var count = rnd.Next(_variantMinOreCount, _variantMaxOreCount + 1);
                    Ores.Add(ore, count);
                }
            }

            ResetOres();
        }

        public void TakeOres(Dictionary<int, int> ores)
        {
            var currentOres = new Dictionary<int, int>(CurrentOres);

            if (ores is null)
                return;

            foreach (var item in ores)
            {
                if (currentOres.TryGetValue(item.Key, out var currentCount) == true)
                    currentOres[item.Key] = Math.Max(_variantMinOreCount, currentCount - item.Value);
            }

            CurrentOres = currentOres;
            LastTake = DateTime.UtcNow;
            IsPatrial = true;
        }

        public void ResetOres()
        {
            CurrentOres = new(Ores);
            IsPatrial = false;
        }
    }
}

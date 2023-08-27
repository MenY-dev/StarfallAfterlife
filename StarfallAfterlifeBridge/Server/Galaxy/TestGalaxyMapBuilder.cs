using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class TestGalaxyMapBuilder : GalaxyMapBuilder
    {

        public static GalaxyMap Map
        {
            get
            {
                if (map == null)
                {
                    lock (locker)
                    {
                        if (map == null)
                        {
                            map = new TestGalaxyMapBuilder().Create();
                        }
                    }
                }

                return map;
            }
        }

        private static GalaxyMap map;
        private static object locker = new();

        public override GalaxyMap Create(int seed, float density = 5000, float radius = 300000) => Map;

        public GalaxyMap Create()
        {
            GalaxyMap map = JsonHelpers.DeserializeUnbuffered<GalaxyMap>(
                File.ReadAllText(Path.Combine(".", "Database", "DefaultMap.json")));

            map.Init();

            return map;
        }
    }
}

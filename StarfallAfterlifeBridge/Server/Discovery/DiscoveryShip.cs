using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryShip
    {
        public int Id { get; set; }

        public int Hull { get; set; }

        public void LoadFromJson(JsonNode doc)
        {
            if (doc is null)
                return;

            Id = (int?)doc["id"] ?? -1;
        }
    }
}

using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryDetachment
    {
        public List<DiscoveryDetachmentSlot> Slots { get; } = new();

        public void LoadFromJson(JsonNode doc)
        {
            Slots.Clear();
        }
    }
}

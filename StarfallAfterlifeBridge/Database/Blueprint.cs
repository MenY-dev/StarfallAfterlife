using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class Blueprint : SfaItem
    {
        public TechType TechType { get; set; }

        public Blueprint(JsonNode doc, JsonNode info) : base(doc, info)
        {
            TechType = (TechType?)(byte?)doc["techtype"] ?? TechType.Unknown;
            ItemType = InventoryItemType.Equipment;
        }
    }
}

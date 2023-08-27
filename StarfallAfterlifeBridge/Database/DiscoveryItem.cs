using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class DiscoveryItem : SfaItem
    {
        public DiscoveryItem(JsonNode doc, JsonNode info) : base(doc, info)
        {
            ItemType = (InventoryItemType?)(byte?)doc["item_type"] ?? InventoryItemType.None;
        }
    }
}

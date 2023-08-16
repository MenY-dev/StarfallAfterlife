using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class EquipmentBlueprint : Blueprint
    {
        public EquipmentBlueprint(JsonNode doc, JsonNode info) : base(doc, info)
        {
            ItemType = InventoryItemType.Equipment;
        }
    }
}

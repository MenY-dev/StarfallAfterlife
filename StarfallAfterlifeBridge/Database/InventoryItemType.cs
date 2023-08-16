using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum InventoryItemType : byte
    {
        ShipProject = 0,
        ItemProject = 1,
        Equipment = 2,
        DiscoveryItem = 3,
        None = 254,
    }
}

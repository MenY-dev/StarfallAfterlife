using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public struct InventoryItemInfo
    {
        public InventoryItemType Type;
        public int Id;
        public int Count;
        public int IGCPrice;
        public int BGCPrice;
        public string UniqueData;
    }
}

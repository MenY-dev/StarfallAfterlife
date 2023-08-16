using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class ShopInfo
    {
        public string ShopName { get; set; }

        public string StocName { get; set; }

        public List<InventoryItem> Items { get; set; } = new();
    }
}

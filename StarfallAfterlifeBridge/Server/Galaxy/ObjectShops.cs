using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class ObjectShops
    {
        public GalaxyMapObjectType ObjectType { get; set; } = GalaxyMapObjectType.None;

        public int ObjectId { get; set; } = -1;

        public List<ShopInfo> Shops { get; set; } = new();
    }
}

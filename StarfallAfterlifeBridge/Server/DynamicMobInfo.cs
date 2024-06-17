using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public class DynamicMob
    {
        public DiscoveryMobInfo Info {  get; set; }

        public DynamicMobType Type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DiscoveryEngine
    {
        public List<DiscoveryFleet> Ships = new List<DiscoveryFleet>();

        public virtual void Run()
        {

        }

        protected virtual void UpdateShips()
        {

        }
    }
}

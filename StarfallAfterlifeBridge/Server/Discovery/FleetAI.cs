using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class FleetAI
    {
        public virtual bool IsConnected { get; set; } = false;

        public DiscoveryFleet Fleet { get; private set; }

        public StarSystem System => Fleet.System;

        public virtual void Connect(DiscoveryFleet fleet)
        {
            Fleet = fleet;
            IsConnected = true;
        }

        public virtual void Disconnect()
        {
            Fleet = null;
            IsConnected = false;
        }

        public virtual void Update()
        {

        }
    }
}
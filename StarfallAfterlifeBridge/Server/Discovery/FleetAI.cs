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

        public virtual void Connect(DiscoveryFleet fleet)
        {
            IsConnected = true;
        }

        public virtual void Disconnect()
        {
            IsConnected = false;
        }

        public virtual void Update()
        {

        }
    }
}
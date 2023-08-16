using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IFleetListener : IDiscoveryListener
    {
        void OnFleetMoved(DiscoveryFleet fleet);

        void OnFleetRouteChanged(DiscoveryFleet fleet);

        void OnFleetDataChanged(DiscoveryFleet fleet);
    }
}

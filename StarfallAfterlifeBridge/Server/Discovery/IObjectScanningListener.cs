using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IObjectScanningListener : IDiscoveryListener
    {
        void OnScanningStarted(DiscoveryFleet fleet, StarSystemObject toScan, float seconds);

        void OnScanningFinished(DiscoveryFleet fleet, StarSystemObject toScan);

        void OnScanningCanceled(DiscoveryFleet fleet, StarSystemObject toScan);
    }
}

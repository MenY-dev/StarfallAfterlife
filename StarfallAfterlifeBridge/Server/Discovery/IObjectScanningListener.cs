using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IObjectScanningListener : IDiscoveryListener
    {
        void OnScanningStateChanged(DiscoveryFleet fleet, ScanInfo info);
        void OnSecretObjectRevealed(DiscoveryFleet fleet, SecretObject secretObject);
    }
}

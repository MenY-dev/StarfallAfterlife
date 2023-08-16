using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IUserFleetListener : IDiscoveryListener
    {
        void OnSystemExplorationChanged(UserFleet fleet, SystemHex newHex, int vision);
    }
}

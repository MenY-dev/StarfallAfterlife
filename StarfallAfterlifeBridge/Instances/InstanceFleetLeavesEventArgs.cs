using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceFleetLeavesEventArgs : EventArgs
    {
        public InstanceInfo Instance { get; }
        public DiscoveryObjectType FleetType { get; }
        public int FleetId { get; }
        public SystemHex Hex { get; }

        public InstanceFleetLeavesEventArgs(InstanceInfo instance, DiscoveryObjectType fleetType, int fleetId, SystemHex hex)
        {
            Instance = instance;
            FleetType = fleetType;
            FleetId = fleetId;
            Hex = hex;
        }
    }
}

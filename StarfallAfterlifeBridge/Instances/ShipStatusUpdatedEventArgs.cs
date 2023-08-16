using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class ShipStatusUpdatedEventArgs : EventArgs
    {
        public InstanceInfo Instance { get; }

        public int ShipId { get; }

        public string ShipData { get; }

        public string ShipStats { get; }

        public ShipStatusUpdatedEventArgs(InstanceInfo instance, int shipId, string shipData, string shipStats)
        {
            Instance = instance;
            ShipId = shipId;
            ShipData = shipData;
            ShipStats = shipStats;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class RankedFleetRequestEventArgs : EventArgs
    {
        public string InstanceAuth { get; }

        public int FleetId { get; }

        public RankedFleetRequestEventArgs(string instanceAuth, int fleetId)
        {
            InstanceAuth = instanceAuth;
            FleetId = fleetId;
        }
    }
}

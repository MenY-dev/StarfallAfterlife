using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Instances
{
    public class SpecialFleetRequestEventArgs
    {
        public string Auth { get; }
        public string FleetName { get; }

        public SpecialFleetRequestEventArgs(string auth, string fleetName)
        {
            Auth = auth;
            FleetName = fleetName;
        }
    }
}

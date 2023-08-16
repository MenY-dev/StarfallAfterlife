using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class SpecialFleetResponseEventArgs : EventArgs
    {
        public string Auth { get; }
        public string FleetName { get; }
        public JsonNode Data { get; }

        public SpecialFleetResponseEventArgs(string auth, string fleetName, JsonNode data)
        {
            Auth = auth;
            FleetName = fleetName;
            Data = data;
        }
    }
}

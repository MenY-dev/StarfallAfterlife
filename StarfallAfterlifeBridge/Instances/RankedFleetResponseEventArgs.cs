using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class RankedFleetResponseEventArgs : EventArgs
    {
        public string InstanceAuth { get; }

        public int FleetId { get; }

        public JsonNode Data { get; }

        public RankedFleetResponseEventArgs(string instanceAuth, int mobId, JsonNode data)
        {
            InstanceAuth = instanceAuth;
            FleetId = mobId;
            Data = data;
        }
    }
}

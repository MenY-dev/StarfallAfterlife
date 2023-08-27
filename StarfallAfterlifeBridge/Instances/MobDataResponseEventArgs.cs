using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class MobDataResponseEventArgs : EventArgs
    {
        public string InstanceAuth { get; }

        public int MobId { get; }

        public JsonNode Data { get; }

        public MobDataResponseEventArgs(string instanceAuth, int mobId, JsonNode data)
        {
            InstanceAuth = instanceAuth;
            MobId = mobId;
            Data = data;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class MobDataRequestEventArgs : EventArgs
    {
        public string InstanceAuth { get; }
        public int MobId { get; }

        public MobDataRequestEventArgs(string instanceAuth, int mobId)
        {
            InstanceAuth = instanceAuth;
            MobId = mobId;
        }
    }
}

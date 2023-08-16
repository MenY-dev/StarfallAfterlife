using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceActionEventArgs : EventArgs
    {
        public InstanceInfo Instance { get; }

        public string Action { get; }

        public string Data { get; }

        public InstanceActionEventArgs(InstanceInfo instance, string action, string data)
        {
            Instance = instance;
            Action = action;
            Data = data;
        }
    }
}

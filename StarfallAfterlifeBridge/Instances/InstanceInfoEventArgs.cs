using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceInfoEventArgs : EventArgs
    {
        public InstanceInfo Instance { get; }
        public InstanceState State { get; } = InstanceState.None;


        public InstanceInfoEventArgs(InstanceInfo instance)
        {
            Instance = instance;
        }

        public InstanceInfoEventArgs(InstanceInfo instance, InstanceState state)
        {
            Instance = instance;
            State = state;
        }
    }
}

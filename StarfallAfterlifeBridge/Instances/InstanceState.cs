using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public enum InstanceState : byte
    {
        None = 0,
        Created = 1,
        Started = 2,
        Finished = 3,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class RewardForEvenRequestEventArgs : EventArgs
    {
        public string Auth { get; }

        public RewardForEvenRequestEventArgs(string auth)
        {
            Auth = auth;
        }
    }
}

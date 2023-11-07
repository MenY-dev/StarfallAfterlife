using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class RewardForEvenResponseEventArgs : EventArgs
    {
        public string Auth { get; }

        public string Reward { get; }

        public RewardForEvenResponseEventArgs(string auth, string reward)
        {
            Auth = auth;
            Reward = reward;
        }
    }
}

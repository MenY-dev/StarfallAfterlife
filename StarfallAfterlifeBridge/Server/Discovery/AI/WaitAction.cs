using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class WaitAction : AIAction
    {
        public TimeSpan Duration { get; }

        public WaitAction(TimeSpan duration)
        {
            Duration = duration;
        }

        public override void Update()
        {
            base.Update();

            if (TotalTime > Duration)
                State = AIActionState.Completed;
        }
    }
}

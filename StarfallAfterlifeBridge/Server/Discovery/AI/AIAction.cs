using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class AIAction
    {
        public FleetAI AI { get; set; }

        protected DiscoveryFleet Fleet => AI?.Fleet;

        public AIActionState State { get; set; }

        public string Name { get; set; }

        protected DateTime StartTime { get; set; }

        protected DateTime LastUpdateTime { get; set; }

        protected DateTime CurrentTime { get; set; }

        protected TimeSpan TotalTime { get; set; }

        protected TimeSpan DeltaTime { get; set; }

        public virtual void Start()
        {
            State = AIActionState.Started;
            StartTime = DateTime.Now;
        }

        public virtual void Update()
        {
            CurrentTime = DateTime.Now;
            DeltaTime = CurrentTime - LastUpdateTime;
            TotalTime = CurrentTime - StartTime;
        }

        public virtual void Stop()
        {
            if (State is AIActionState.Started)
                State = AIActionState.Failed;
        }
    }
}

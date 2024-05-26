using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class AIWatchdog
    {
        public string Name { get; set; }

        public TimeSpan Period { get; set; }

        public bool InvokeAtStart { get; set; }

        public DateTime NextTick { get; set; }

        public Action<AIWatchdog> Action { get; set; }

        public void Update()
        {
            var now = DateTime.UtcNow;

            if (now >= NextTick)
            {
                NextTick = now + Period;
                Action?.Invoke(this);
            }
        }
    }
}

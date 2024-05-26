using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public interface IAINode
    {
        FleetAI AI { get; set; }

        AINodeState State { get; set; }

        string Name { get; set; }

        void Start();

        void Update();

        void Stop();
    }
}

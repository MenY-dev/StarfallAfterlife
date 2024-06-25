using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class CaravanTutorBattle : DiscoveryBattle
    {
        public override void Init()
        {
            base.Init();

            InstanceInfo.Type = Instances.InstanceType.CaravanTutorial;
        }
    }
}

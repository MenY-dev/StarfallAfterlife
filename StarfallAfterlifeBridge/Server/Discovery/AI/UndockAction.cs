using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class UndockAction : AIAction
    {
        public UndockAction()
        {

        }

        public override void Start()
        {
            base.Start();

            if (Fleet is DiscoveryFleet fleet)
            {
                fleet.DockObjectId = -1;
                fleet.DockObjectType = DiscoveryObjectType.None;
                fleet.Broadcast<IFleetListener>(l => l.OnFleetDataChanged(fleet));
                State = AINodeState.Completed;
            }
            else
            {
                State = AINodeState.Failed;
            }
        }
    }
}

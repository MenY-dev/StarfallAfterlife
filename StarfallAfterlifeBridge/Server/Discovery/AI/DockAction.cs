using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery.AI
{
    public class DockAction : AIAction
    {
        public DiscoveryObjectType ObjectType { get; }

        public int ObjectId { get; }


        public DockAction(DiscoveryObjectType objectType, int objectId)
        {
            ObjectType = objectType;
            ObjectId = objectId;
        }

        public override void Start()
        {
            base.Start();

            if (Fleet is DiscoveryFleet fleet &&
                fleet?.System?.GetObject(ObjectId, ObjectType) is StarSystemObject obj)
            {
                fleet.DockObjectId = ObjectId;
                fleet.DockObjectType = ObjectType;
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

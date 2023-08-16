using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class DockableObject : StarSystemObject
    {
        public ObjectStoragesCollection Storages { get; } = new();

        public List<TaskBoardEntry> TaskBoard { get; } = new();

    }
}

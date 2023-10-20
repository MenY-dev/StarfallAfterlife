using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public interface IExplorationListener
    {
        void OnObjectScanned(StarSystemObject systemObject);

        void OnSystemExplored(int systemId);

        void OnObjectExplored(int systemId, DiscoveryObjectType objectType, int objectId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IStarSystemObjectListener : IDiscoveryListener
    {
        void OnObjectSpawned(StarSystemObject obj);
        void OnObjectDestroed(StarSystemObject obj);
    }
}

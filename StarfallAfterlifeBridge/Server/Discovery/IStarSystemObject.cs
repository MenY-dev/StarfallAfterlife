using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public interface IStarSystemObject
    {
        StarSystem System { get; }

        void OnSystemChanged(StarSystem system);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public interface IGalaxyMapObject
    {
        public GalaxyMapObjectType ObjectType { get; }

        public int Id { get; set; }

        public int X { get; }

        public int Y { get; }
    }
}

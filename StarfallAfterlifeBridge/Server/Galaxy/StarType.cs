using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public enum StarType : byte
    {
        BlackHole = 0,
        RedDwarf = 1,
        YellowDwarf = 2,
        OrangeDwarf = 3,
        BlueDwarf = 4,
        UltracoldDwarf = 5,
        Pulsar = 6,
        Neutron = 7,
        BlueGiant = 8,
        RedGiant = 9,
        Double = 10,
        Planetary = 11,
        RedSupergiant = 12,
        YellowSupergiant = 13,
        BlueSupergiant = 14,
        None = 254,
    }
}

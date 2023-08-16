using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    [Flags]
    public enum TechType : byte
    {
        Unknown = 0,
        Ship = 1,
        Engine = 2,
        Shield = 4,
        Beam = 8,
        Missile = 16,
        Ballistic = 32,
        Engineering = 64 | Shield,
        Carrier = 128,
        Weapons = Beam | Missile | Ballistic | Carrier,
        Ammo = Ship | Beam | Missile | Ballistic | Engineering,
        Armor = Ship | Shield | Weapons | Engineering,
    }
}

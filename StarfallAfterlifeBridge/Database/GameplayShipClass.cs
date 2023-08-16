using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public enum GameplayShipClass : byte
    {
        Unknown = 0,
        Frigate = 1,
        Cruiser = 2,
        Battlecruiser = 3,
        Battleship = 4,
        Dreadnought = 5,
        Carrier = 6,
        SellingSlot = 7,
    }
}

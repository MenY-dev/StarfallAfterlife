using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public enum DiscoveryObjectType : byte
    {
        None = 0,
        UserFleet = 1,
        Planet = 2,
        Repairstation = 3,
        Fuelstation = 4,
        AiFleet = 5,
        Mothership = 6,
        Trash = 7,
        Asteroid = 8,
        Blackmarket = 9,
        AttackEventInstance = 10,
        PiratesStation = 15,
        InstanceBattle = 16,
        RichAsteroids = 17,
        Nebula = 18,
        UserPhantom = 19,
        WarpBeacon = 20,
        MiningStation = 21,
        MessageBeacon = 22,
        MinerMothership = 23,
        ScienceStation = 25,
        QuickTravelGate = 26,
        PiratesOutpost = 27,
        SecretObject = 28,
        CustomInstance = 29,
        Tradestation = 30,
        HouseActionHolder = 31,
    }
}

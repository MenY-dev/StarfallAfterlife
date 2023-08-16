using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public enum GalaxyMapObjectType : byte
    {
        [Display(Name= "None")]
        None = 0,

        [Display(Name = "Planet")]
        Planet = 2,

        [Display(Name = "Repair Station")]
        RepairStation = 3,

        [Display(Name = "Fuel Station")]
        FuelStation = 4,

        [Display(Name = "Mothership")]
        Mothership = 6,

        [Display(Name = "Blackmarket")]
        Blackmarket = 9,

        [Display(Name = "Pirates Station")]
        PiratesStation = 15,

        [Display(Name = "Rich Asteroids")]
        RichAsteroids = 17,

        [Display(Name = "Warp Beacon")]
        WarpBeacon = 20,

        [Display(Name = "Mining Station")]
        MiningStation = 21,

        [Display(Name = "Message Beacon")]
        MessageBeacon = 22,

        [Display(Name = "Miner Mothership")]
        MinerMothership = 23,

        [Display(Name = "Science Station")]
        ScienceStation = 25,

        [Display(Name = "Quick Travel Gate")]
        QuickTravelGate = 26,

        [Display(Name = "Pirates Outpost")]
        PiratesOutpost = 27,

        [Display(Name = "NoSecret Objectne")]
        SecretObject = 28,

        [Display(Name = "Trade Station")]
        TradeStation = 30,
    }
}

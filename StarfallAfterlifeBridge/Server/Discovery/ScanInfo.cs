using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public struct ScanInfo
    {
        public ScanState State { get; set; }

        public bool SectorScanning { get; set; }

        public float Time { get; set; }

        public StarSystemObject SystemObject { get; set; }

        public SystemHex Sector { get; set; }
    }
}

using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public class ObjectSelectionInfo
    {
        public StarSystemObject Target { get; set; }

        public bool Scanned { get; set; }

        public bool ScanningStarted { get; set; }

        public bool ScanningFinished { get; set; }

        public float ScanningTime { get; set; }

        public List<string> Actions { get; } = new();

    }
}

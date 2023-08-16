using StarfallAfterlife.Bridge.Instances;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {
        public InstanceManager ActiveInstanceManager { get; set; }

        public void StartInstanceManager()
        {
            ActiveInstanceManager?.Stop();

            ActiveInstanceManager = new InstanceManager()
            {
                GameExeLocation = GameExeLocation,
                WorkingDirectory = WorkingDirectory
            };

            ActiveInstanceManager.Start();
        }
    }
}

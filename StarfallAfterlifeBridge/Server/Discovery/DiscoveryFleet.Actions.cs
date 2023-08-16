using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public partial class DiscoveryFleet
    {
        protected StarSystemObject ObjectToScan;

        protected DateTime ScanEndTime;

        protected SystemHex ScanStartHex;

        protected virtual void TickActions()
        {
            TickScanObject();
        }

        public void ScanObject(int id, DiscoveryObjectType type) =>
            ScanObject(System?.GetObject(id, type));

        public void ScanObject(StarSystemObject obj)
        {
            if (obj is null)
                return;

            var seconds = 5f;
            ObjectToScan = obj;
            ScanEndTime = DateTime.Now.AddSeconds(seconds);
            ScanStartHex = Hex;
            Broadcast<IObjectScanningListener>(l => l.OnScanningStarted(this, ObjectToScan, seconds));
        }

        public void CancelScanning()
        {
            var obj = ObjectToScan;
            ObjectToScan = null;
            Broadcast<IObjectScanningListener>(l => l.OnScanningCanceled(this, obj));
        }

        public void TickScanObject()
        {
            if (ObjectToScan is not null)
            {
                if (Hex != ScanStartHex)
                {
                    CancelScanning();
                }
                else if (DateTime.Now >= ScanEndTime)
                {
                    var obj = ObjectToScan;
                    ObjectToScan = null;
                    Broadcast<IObjectScanningListener>(l => l.OnScanningFinished(this, obj));
                }
            }
        }
    }
}

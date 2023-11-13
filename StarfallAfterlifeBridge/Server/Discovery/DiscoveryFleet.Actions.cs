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

        protected ScanInfo ScanInfo;

        protected SystemHex ScanStartHex;

        protected DateTime ScanEndTime;

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

            ScanInfo = new()
            {
                State = ScanState.Started,
                SystemObject = ObjectToScan = obj,
                Sector = obj.Hex,
                Time = 5f,
            };

            ScanStartHex = Hex;
            ScanEndTime = DateTime.Now.AddSeconds(ScanInfo.Time);
            Broadcast<IObjectScanningListener>(l => l.OnScanningStateChanged(this, ScanInfo));
        }

        public void ScanSector(SystemHex sector)
        {
            ScanInfo = new()
            {
                State = ScanState.Started,
                SectorScanning = true,
                Sector = sector,
                Time = 5f,
            };

            ScanStartHex = Hex;
            ScanEndTime = DateTime.Now.AddSeconds(ScanInfo.Time);
            Broadcast<IObjectScanningListener>(l => l.OnScanningStateChanged(this, ScanInfo));
        }

        public void CancelScanning()
        {
            var info = ScanInfo;
            info.State = ScanState.Cancelled;
            ScanInfo = new();
            Broadcast<IObjectScanningListener>(l => l.OnScanningStateChanged(this, info));
        }

        public void TickScanObject()
        {
            if (ScanInfo.State is ScanState.Started)
            {
                if (Hex != ScanStartHex)
                {
                    CancelScanning();
                }
                else if (DateTime.Now >= ScanEndTime)
                {
                    var info = ScanInfo;
                    info.State = ScanState.Finished;
                    ScanInfo = new();
                    Broadcast<IObjectScanningListener>(l => l.OnScanningStateChanged(this, info));

                    if (info.SectorScanning == true &&
                        System?.GetObjectsAt<SecretObject>(info.Sector, false)?.FirstOrDefault() is SecretObject secret)
                        Broadcast<IObjectScanningListener>(l => l.OnSecretObjectRevealed(this, secret));
                }
            }
        }

        public StarSystemBattle GetBattle() => System?.GetBattle(this);
    }
}

using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class PatrollingAI : FleetAI
    {
        public double WaitingTime { get; set; } = 3;

        public int CurrentWaypoint { get; protected set; } = 0;

        public bool IsInWaiting { get; protected set; } = false;

        public DateTime WaitingStartTime { get; protected set; }

        protected DiscoveryFleet Fleet { get; set; }

        public override void Connect(DiscoveryFleet fleet)
        {
            base.Connect(fleet);

            if (Fleet is not null)
                Fleet.TargetLocationReached -= OnTargetLocationReached;

            Fleet = fleet;

            if (Fleet is not null)
                Fleet.TargetLocationReached += OnTargetLocationReached;

            IsInWaiting = true;
            WaitingStartTime = DateTime.Now;
        }


        public override void Disconnect()
        {
            base.Disconnect();

            if (Fleet is not null)
                Fleet.TargetLocationReached -= OnTargetLocationReached;

            Fleet = null;
        }

        public override void Update()
        {
            base.Update();

            if (IsConnected == false ||
                Fleet.EngineEnabled == false ||
                Fleet.State != FleetState.InGalaxy)
                return;

            if (IsInWaiting == true && (DateTime.Now - WaitingStartTime).TotalSeconds > WaitingTime)
            {
                IsInWaiting = false;

                Fleet.MoveTo(SystemHexMap.ArrayIndexToHex(
                    new Random().Next(0, SystemHexMap.HexesCount)));
            }
        }

        private void OnTargetLocationReached(object sender, EventArgs e)
        {
            if (IsConnected == false)
                return;

            IsInWaiting = true;
            WaitingStartTime = DateTime.Now;
        }
    }
}

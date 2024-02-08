using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class RankedPlayerInfo
    {
        public SfaServerClient Player { get; set; }

        public int Team { get; set; } = -1;

        public int FleetId { get; set; }

        public RankedLobbyUserStatus Status { get; set; } = RankedLobbyUserStatus.None;
    }
}

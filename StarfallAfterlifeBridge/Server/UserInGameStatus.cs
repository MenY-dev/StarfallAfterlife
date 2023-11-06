using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum UserInGameStatus : byte
    {
        CharMainMenu = 0,
        CharSearchingForGame = 1,
        CharInBattle = 2,
        CharInDiscovery = 3,
        RankedMainMenu = 128,
        RankedSearchingForGame = 129,
        RankedInBattle = 130,
        None = 250,
    }
}

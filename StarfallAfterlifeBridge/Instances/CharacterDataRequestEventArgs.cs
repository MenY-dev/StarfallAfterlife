using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class CharacterDataRequestEventArgs : EventArgs
    {
        public int CharacterId { get; }
        public string GameMode { get; }
        public bool IncludeDestroyedShips { get; }

        public CharacterDataRequestEventArgs(int characterId, string gameMode, bool includeDestroyedShips)
        {
            CharacterId = characterId;
            GameMode = gameMode;
            IncludeDestroyedShips = includeDestroyedShips;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class AddCharacterShipsXpEventArgs : EventArgs
    {
        public int CharacterId { get; }

        public Dictionary<int, int> Ships { get; }

        public AddCharacterShipsXpEventArgs(int characterId, Dictionary<int, int> ships)
        {
            CharacterId = characterId;
            Ships = ships;
        }
    }
}

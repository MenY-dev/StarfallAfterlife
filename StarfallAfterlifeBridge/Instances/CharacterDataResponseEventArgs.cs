using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class CharacterDataResponseEventArgs : EventArgs
    {
        public int CharacterId { get; }

        public string Data { get; }

        public CharacterDataResponseEventArgs(int characterId, string data)
        {
            CharacterId = characterId;
            Data = data;
        }
    }
}

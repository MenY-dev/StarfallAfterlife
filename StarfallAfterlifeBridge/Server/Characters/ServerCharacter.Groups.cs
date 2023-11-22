using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public partial class ServerCharacter
    {
        public CharacterParty Party { get; set; }

        public void OnAddedToGroup(CharacterParty group)
        {
            Party = group;
        }

        public void OnGroupUpdated(CharacterParty group)
        {

        }

        public void OnRemovedFromGroup(CharacterParty group)
        {
            Party = null;
        }
    }
}

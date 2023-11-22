using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public class CharacterPartyMember
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public PartyMemberStatus Status { get; set; }

        public int CurrentStarSystem { get;  set; }
    }
}

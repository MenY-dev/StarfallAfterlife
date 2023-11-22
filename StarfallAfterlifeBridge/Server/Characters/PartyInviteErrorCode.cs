using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public enum PartyInviteErrorCode : byte
    {
        Success = 0,
        NoCharacter = 1,
        AllreadyInParty = 2,
        MemberLimit = 3,
    }
}

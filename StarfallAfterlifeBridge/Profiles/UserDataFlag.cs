using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    [Flags]
    public enum UserDataFlag : uint
    {
        None                   = 0,
        CharacterInfo          = 0x2,
        Boosters               = 0x4,
        WeeklyQuests           = 0x8,
        DiscoveryBattleInfo    = 0x10,
        Ships                  = 0x40,
        Inventory              = 0x80,
        Crafting               = 0x100,
        CheckedEvents          = 0x200,
        Achievements           = 0x400,
        Detachments            = 0x800,
        ProjectsResearch       = 0x1000,
        FactionShop            = 0x2000,
        ActiveShips            = 0x4000,
        CompletedQuests        = 0x8000,
        BGShopItems            = 0x10000,
        CharactRewards         = 0x20000,
        SpecOps                = 0x40000,
        SpecOpsRewards         = 0x80000,
        CharactRewardQueue     = 0x100000,
        All                    = 0xFFFFFFFF
    }
}

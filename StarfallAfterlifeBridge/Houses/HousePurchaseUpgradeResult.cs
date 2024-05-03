using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public enum HousePurchaseUpgradeResult : byte
    {
        Success = 0,
        NotEnoughCurrency = 1,
        NoPermission = 2,
        AlreadyOpened = 3,
        RequirementsNotMet = 4,
        Unknown = 254,
    }
}

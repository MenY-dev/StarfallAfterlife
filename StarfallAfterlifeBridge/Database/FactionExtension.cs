using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public static class FactionExtension
    {
        public static bool IsEnemy(this Faction firstFaction, Faction secondFaction)
        {
            if ((byte)firstFaction < (byte)Faction.Noxophytes &&
                (byte)secondFaction < (byte)Faction.Noxophytes)
                return firstFaction != secondFaction;

            return false;
        }

        public static bool IsPirates(this Faction self)
        {
            if (self is Faction.Pyramid or Faction.Screechers or Faction.Nebulords)
                return true;

            return false;
        }

        public static bool IsMainFaction(this Faction self)
        {
            if (self is Faction.Deprived or Faction.Eclipse or Faction.Vanguard)
                return true;

            return false;
        }
    }
}

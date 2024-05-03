using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public static class HouseServerExtensions
    {
        public static HouseMember GetMember(this SfHouseInfo self, ServerCharacter character) =>
            GetMember(self?.House, character);

        public static HouseMember GetMember(this SfHouse self, ServerCharacter character)
        {
            if (self is null || character is null)
                return null;

            var playerId = character.ProfileId;
            var charId = character.Guid;

            return self.Members.Values.FirstOrDefault(m =>
                m.PlayerId == playerId &&
                m.CharacterId == charId);
        }

        public static HouseMember AddMember(this SfHouse self, ServerCharacter character)
        {
            if (self is null || character is null || character.GetHouse() is not null)
                return null;

            var member = new HouseMember()
            {
                Name = character.UniqueName,
                PlayerId = character.ProfileId,
                CharacterId = character.Guid,
            };

            self.AddMember(member);
            return member;
        }

        public static HouseMember RemoveMember(this SfHouse self, ServerCharacter character)
        {
            if (self is null || character is null)
                return null;

            var playerId = character.ProfileId;
            var charId = character.Guid;

            if (playerId == Guid.Empty ||
                charId == Guid.Empty)
                return null;

            return self.RemoveMember(playerId, charId);
        }

        public static bool IsHouseOwner(this ServerCharacter self, SfHouseInfo info) =>
            IsHouseOwner(self, info?.House);

        public static bool IsHouseOwner(this ServerCharacter self, SfHouse house)
        {
            var member = GetMember(house, self);

            if (member is null || house.Ranks.Count < 1)
                return false;

            var ownerRank = house.Ranks.MinBy(h => h.Order);
            return member.Rank == ownerRank?.Id;
        }

        public static HouseRankPermission GetHousePermissions(this ServerCharacter self, SfHouseInfo houseInfo) =>
            GetHousePermissions(self, houseInfo?.House);

        public static HouseRankPermission GetHousePermissions(this ServerCharacter self, SfHouse house)
        {
            var member = GetMember(house, self);

            if (member is null || house.Ranks.Count < 1)
                return HouseRankPermission.None;

            var rank = house.Ranks.FirstOrDefault(h => h.Id == member.Rank);
            return rank?.Permissions ?? HouseRankPermission.None;
        }

        public static SfHouse GetHouse(this ServerCharacter self) =>
            GetHouseInfo(self)?.House;

        public static SfHouseInfo GetHouseInfo(this ServerCharacter self)
        {
            if (self is not null && 
                self.Guid != Guid.Empty &&
                self.ProfileId != Guid.Empty &&
                self.Realm?.HouseDatabase is SfHouseDatabase dtb)
                return dtb.GetHouse(self.ProfileId, self.Guid);

            return null;
        }


    }
}

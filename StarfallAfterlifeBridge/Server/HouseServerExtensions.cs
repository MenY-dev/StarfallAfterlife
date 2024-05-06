using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Quests;
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

        public static Task<DiscoveryQuest> CreateHouseQuest(this ServerCharacter character, string identity)
        {
            if (character is null ||
                string.IsNullOrWhiteSpace(identity))
                return Task.FromResult<DiscoveryQuest>(null);

            return Task<DiscoveryQuest>.Factory.StartNew(() =>
            {
                DiscoveryQuest quest = null;

                character.DiscoveryClient?.Server?.UseQuestGenerator(gen =>
                {
                    var galaxyMap = gen.Realm.GalaxyMap;
                    var extraMap = gen.ExtraMap;
                    var system = character.Fleet?.System?.Info;

                    if (galaxyMap is null ||
                        extraMap is null ||
                        galaxyMap.Systems is null or { Count: < 1 })
                        return;

                    try
                    {
                        if (system is null ||
                            system.Level < character.AccessLevel)
                        {
                            system = extraMap.GetCircle(character.AccessLevel)?.GetStartSystem(character.Faction) ??
                                     galaxyMap.Systems.FirstOrDefault();
                        }

                        // Search for a random system nearby
                        system = galaxyMap
                            .GetSystemsArround(system.Id, 3, true)
                            .ToList()
                            .Randomize(new Random().Next())
                            .FirstOrDefault().Key;

                        quest = gen.GenerateHouseTask(identity, system.Id);
                    }
                    catch { }
                });

                return quest;
            });
        }

        public static HouseDoctrine StartDoctrine(this ServerCharacter character, int doctrineId)
        {
            if (character is null)
                return null;

            var dtb = character?.DiscoveryClient.Server.Realm.Database ?? SfaDatabase.Instance;

            if (character.GetHouseInfo() is SfHouseInfo houseInfo &&
                houseInfo.House is SfHouse house &&
                character.GetHousePermissions(house).HasFlag(HouseRankPermission.StartDoctrine) == true &&
                dtb.GetHouseDoctrine(doctrineId) is HouseDoctrineInfo doctrineInfo &&
                doctrineInfo.GetQuestLogic(house, dtb) is QuestLogicInfo questLogic)
            {
                var doctrine = new HouseDoctrine
                {
                    Info = doctrineInfo,
                    EndTime = DateTime.UtcNow.AddHours(doctrineInfo.QuestDuration),
                    Target = questLogic.Conditions?.FirstOrDefault().ProgressRequire ?? 0,
                };

                if (house.AddDoctrine(doctrine) == true)
                    return doctrine;
            }

            return null;
        }

        public static bool TakeHouseEffects(this ServerCharacter character) =>
            TakeHouseEffects(character, character?.GetHouse());

        public static bool TakeHouseEffects(this ServerCharacter character, SfHouseInfo houseInfo) =>
            TakeHouseEffects(character, houseInfo?.House);

        public static bool TakeHouseEffects(this ServerCharacter character, SfHouse house) =>
            TakeHouseEffects(character, house?.GetMember(character));

        public static bool TakeHouseEffects(this ServerCharacter character, HouseMember member)
        {
            var result = false;
            var client = character?.DiscoveryClient;

            if (client is null || member is null)
                return result;

            var dtb = client.Server.Realm.Database ?? SfaDatabase.Instance;
            var currentTime = DateTime.UtcNow;

            if (member.Effects is List<HouseEffect> effects)
            {
                foreach (var item in effects)
                {
                    var time = (item.EndTime - currentTime).TotalHours;

                    if (time > 0 && dtb.GetHouseEffect(item.Id) is HouseEffectInfo effectInfo)
                    {
                        if ("craft_booster".Equals(effectInfo.Type, StringComparison.OrdinalIgnoreCase) == true)
                            client.Invoke(_ => character.AddBooster(1464674507, time));
                        else if ("xp_booster".Equals(effectInfo.Type, StringComparison.OrdinalIgnoreCase) == true)
                            client.Invoke(_ => character.AddBooster(1160329638, time));
                    }

                    result = true;
                }

                effects.Clear();
            }

            return result;
        }
    }
}

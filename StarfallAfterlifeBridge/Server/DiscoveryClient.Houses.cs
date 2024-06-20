using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public void InputFromDiscoveryHousesChannel(
            SfReader reader, DiscoveryClientAction action,
            int systemId, DiscoveryObjectType objectType, int objectId)
        {
            switch (action)
            {
                case DiscoveryClientAction.CreateNewHouse:
                    HandleCreateNewHouse(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.LeaveHouseAction:
                    HandleLeaveHouseAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.KickMemberAction:
                    HandleHouseKickMemberAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.DestroyHouseAction:
                    HandleDestroyHouseAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SendHouseUpdateAction:
                    HandleHouseUpdateAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SetRankPermissionsAction:
                    HandleSetRankPermissionsAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SetHouseMessageOfTheDayAction:
                    HandleSetHouseMessageOfTheDayAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SetHouseURLAction:
                    HandleSetHouseURLAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.HouseInviteCharacterAction:
                    HandleHouseInviteCharacterAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.HouseInviteResponseAction:
                    HandleHouseInviteResponseAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.PurchaseHouseUpgradeAction:
                    HandlePurchaseHouseUpgradeAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.TakeHouseTaskFromPool:
                    HandleTakeHouseTaskFromPoolAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.StartDoctrineAction:
                    HandleHouseStartDoctrineAction(reader, systemId, objectType, objectId); break;

                case DiscoveryClientAction.SetMemberRankAction:
                    HandleHouseSetMemberRankAction(reader, systemId, objectType, objectId); break;
            }
        }

        private void HandleCreateNewHouse(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int mothershipId = reader.ReadInt32();
            string name = reader.ReadShortString(Encoding.UTF8);
            string tag = reader.ReadShortString(Encoding.UTF8);

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    HouseCreationResult result;

                    if (Client is SfaServerClient player &&
                        CurrentCharacter is ServerCharacter character &&
                        player.ProfileId != Guid.Empty &&
                        character.Guid != Guid.Empty &&
                        r.Realm?.HouseDatabase is SfHouseDatabase dtb &&
                        (result = dtb.CreateHouse(name, tag, out var houseInfo)) is HouseCreationResult.Success &&
                        houseInfo.House is SfHouse house)
                    {
                        foreach (var item in (Server?.Realm?.Database ?? SfaDatabase.Instance).HouseRanks)
                            house.Ranks.Add(HouseRank.CreateFromDatabaseInfo(item.Value));

                        house.Faction = character.Faction;
                        house.UpdateTasksPool();
                        var member = house.AddMember(character);
                        member.Rank = (Server?.Realm?.Database ?? SfaDatabase.Instance).GetMaxRank()?.Id ?? 0;
                        houseInfo.Save();

                        SendHouseCreated(result);
                        SendHouseFullInfo(houseInfo.House);
                        character.HouseTag = house.Tag;
                        character.SyncDoctrines();
                        character.UpdateFleetInfo();
                    }
                    else
                    {
                        SendHouseCreated(HouseCreationResult.UserAlreadyInTheHouse);
                    }
                });
            });
        }

        private void HandleLeaveHouseAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        r.Realm?.HouseDatabase is SfHouseDatabase dtb &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.GetMember(character) is HouseMember causer &&
                        houseInfo.House is SfHouse house)
                    {
                        if (house.RemoveMember(character) is HouseMember removedMember)
                        {
                            character.HouseTag = null;
                            SendHouseCharacterLeft(character.UniqueId, CharacterHouseLeftReason.Leave, character.UniqueId);
                            character.SyncDoctrines();
                            character.UpdateFleetInfo();
                            Galaxy?.BeginPreUpdateAction(g => character?.Fleet?.BroadcastFleetDataChanged());

                            foreach (var member in house.Members.Values)
                            {
                                if (Server?.GetCharacter(member) is ServerCharacter memberChar &&
                                    memberChar != character)
                                    memberChar.DiscoveryClient?.Invoke(
                                        c => c.SendHouseCharacterLeft(removedMember.Id, CharacterHouseLeftReason.Leave, removedMember.Id));
                            }
                        }

                        if (house.Members.Count > 0)
                        {
                            houseInfo.Save();
                        }
                        else
                        {
                            dtb.DeleteHouse(houseInfo);
                        }
                    }
                });
            });
        }


        private void HandleHouseKickMemberAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            int memberId = reader.ReadInt32();

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        r.Realm?.HouseDatabase is SfHouseDatabase dtb &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.GetMember(character) is HouseMember causer &&
                        houseInfo.House is SfHouse house)
                    {
                        var targetChar = memberId < -1 ?
                            Server.GetCharacter(house.Members.Values.FirstOrDefault(m => m.Id == memberId)) :
                            Server?.GetCharacter(memberId);

                        if (house.RemoveMember(targetChar) is HouseMember removedMember)
                        {
                            targetChar.HouseTag = null;
                            targetChar?.DiscoveryClient?.SendHouseCharacterLeft(targetChar.UniqueId, CharacterHouseLeftReason.Kicked, causer.Id);
                            targetChar.SyncDoctrines();
                            targetChar.UpdateFleetInfo();
                            Galaxy?.BeginPreUpdateAction(g => targetChar.Fleet?.BroadcastFleetDataChanged());


                            foreach (var member in house.Members.Values)
                            {
                                if (Server?.GetCharacter(member) is ServerCharacter memberChar &&
                                    memberChar != targetChar)
                                    memberChar.DiscoveryClient?.Invoke(
                                        c => c.SendHouseCharacterLeft(removedMember.Id, CharacterHouseLeftReason.Kicked, removedMember.Id));
                            }
                        }

                        if (house.Members.Count > 0)
                        {
                            houseInfo.Save();
                        }
                        else
                        {
                            dtb.DeleteHouse(houseInfo);
                        }
                    }
                });
            });
        }

        private void HandleDestroyHouseAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (r.Realm?.HouseDatabase is SfHouseDatabase dtb &&
                        CurrentCharacter is ServerCharacter character &&
                        character.GetHouse() is SfHouse house &&
                        character.IsHouseOwner(house) == true)
                    {
                        character.HouseTag = null;
                        dtb.DeleteHouse(house);
                        SendHouseCharacterLeft(character.UniqueId, CharacterHouseLeftReason.Leave, character.UniqueId);
                        character.SyncDoctrines();
                        character.UpdateFleetInfo();
                        Galaxy?.BeginPreUpdateAction(g => character?.Fleet?.BroadcastFleetDataChanged());

                        foreach (var member in house.Members.Values)
                        {
                            if (Server?.GetCharacter(member) is ServerCharacter memberChar && memberChar != character)
                            {
                                memberChar.DiscoveryClient?.Invoke(c =>
                                {
                                    c.SendHouseCharacterLeft(member.Id, CharacterHouseLeftReason.HouseDestroyed, member.Id);
                                    c.SendHouseDestroyed();
                                });
                            }
                        }
                    }
                });
            });
        }

        public void HandleHouseUpdateAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter?.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house)
                    {
                        if (house.UpdateTasksPool() == true)
                            houseInfo.Save();

                        SendHouseUpdate(house);
                        UpdateHouseMemberInfo();
                        BroadcastHouseCharacterOnlineStatus(true);
                    }
                });
            });
        }

        private void HandleSetRankPermissionsAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var rankId = reader.ReadInt32();
            var permissionIndex = reader.ReadByte();
            var permission = (HouseRankPermission)(1 << permissionIndex);
            var enabled = reader.ReadBoolean();

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (r.Realm?.HouseDatabase is SfHouseDatabase dtb &&
                        CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.ChangeRank) == true &&
                        house.Ranks.FirstOrDefault(r => r?.Id == rankId) is HouseRank rank)
                    {
                        rank.Permissions = enabled ?
                                           rank.Permissions | permission :
                                           rank.Permissions & ~permission;

                        houseInfo.Save();

                        foreach (var member in house.Members.Values)
                        {
                            if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                                memberChar.DiscoveryClient?.Invoke(
                                    c => c.SendHouseRankPermissionChanged(rankId, permissionIndex, enabled));
                        }
                    }
                });
            });
        }

        private void HandleSetHouseMessageOfTheDayAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var msg = reader.ReadShortString(Encoding.UTF8);

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.ChangeMessageOfTheDay) == true)
                    {
                        house.MessageOfTheDay = msg;
                        houseInfo.Save();

                        foreach (var member in house.Members.Values)
                        {
                            if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                                memberChar.DiscoveryClient?.Invoke(
                                    c => c.SendHouseMessageChanged(msg));
                        }
                    }
                });
            });
        }

        private void HandleSetHouseURLAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var url = reader.ReadShortString(Encoding.UTF8);

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.ChangeMessageOfTheDay) == true)
                    {
                        house.Link = url;
                        houseInfo.Save();

                        foreach (var member in house.Members.Values)
                        {
                            if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                                memberChar.DiscoveryClient?.Invoke(
                                    c => c.SendHouseUrlChanged(url));
                        }
                    }
                });
            });
        }

        private void HandleHouseInviteCharacterAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var targetCharName = reader.ReadShortString(Encoding.UTF8);

            Invoke(() =>
            {
                var targetChar = Server?.GetCharacter(targetCharName);

                if (targetChar is null)
                {
                    SendHouseInviteResult(HouseUserInviteResult.UserNotFound);
                    return;
                }

                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.Invite) == true)
                    {
                        if (house.Members.Count >= house.MaxMembers)
                        {
                            SendHouseInviteResult(HouseUserInviteResult.NoRecruitRights);
                            return;
                        }

                        if (targetChar.GetHouse() is not null)
                        {
                            SendHouseInviteResult(HouseUserInviteResult.AlreadyInHouse);
                            return;
                        }

                        if (targetChar.Faction != house.Faction)
                        {
                            SendHouseInviteResult(HouseUserInviteResult.EnemyFaction);
                            return;
                        }

                        targetChar.DiscoveryClient?.Invoke(
                            c => c.SendHouseInvitation(house, character.UniqueName));

                        SendHouseInviteResult(HouseUserInviteResult.Success);
                    }
                    else
                    {
                        SendHouseInviteResult(HouseUserInviteResult.NoRecruitRights);
                    }
                });
            });
        }

        private void HandleHouseInviteResponseAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var result = reader.ReadBoolean();

            if (result == false)
                return;

            Invoke(() =>
            {
                var character = CurrentCharacter;

                if (character is null)
                {
                    SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.Unknown);
                    return;
                }

                if (character.GetHouse() is not null)
                {
                    SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.AlreadyInHouse);
                    return;
                }

                Server?.RealmInfo?.Use(r =>
                {
                    if (r.Realm?.HouseDatabase is SfHouseDatabase dtb &&
                        dtb.Houses.GetValueOrDefault(objectId) is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house)
                    {
                        if (house.GetMember(character) is not null)
                        {
                            SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.AlreadyInHouse);
                            return;
                        }

                        if (house.Members.Count >= house.MaxMembers)
                        {
                            SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.MembersLimitReached);
                            return;
                        }

                        if (house.AddMember(character) is HouseMember newMember)
                        {
                            newMember.Rank = (Server?.Realm?.Database ?? SfaDatabase.Instance).GetMinRank()?.Id ?? 0;
                            houseInfo.Save();
                            character.HouseTag = house.Tag;
                            SendHouseNewMember(character.UniqueId, newMember.Name, newMember.Rank);

                            foreach (var member in house.Members.Values)
                            {
                                if (Server?.GetCharacter(member) is ServerCharacter memberChar &&
                                    memberChar != character)
                                    memberChar.DiscoveryClient?.Invoke(
                                        c => c.SendHouseNewMember(newMember.Id, newMember.Name, newMember.Rank));
                            }

                            BroadcastHouseMemberInfoChanged();
                            BroadcastHouseMemberRankChanged();
                            BroadcastHouseCharacterOnlineStatus(true);
                            BroadcastHouseFullInfo();

                            character.UpdateFleetInfo();
                            character.SyncDoctrines();
                            Galaxy?.BeginPreUpdateAction(g => character?.Fleet?.BroadcastFleetDataChanged());
                            SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.Success);
                        }
                        else
                        {
                            SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.InviteExpired);
                        }
                    }
                    else
                    {
                        SendHouseInviteResponseResult(objectId, HouseInviteAcceptResult.HouseDestroyed);
                    }
                });
            });
        }

        private void HandlePurchaseHouseUpgradeAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var id = reader.ReadInt32();
            var level = reader.ReadInt32();
            var dtb = Server?.Realm?.Database ?? SfaDatabase.Instance;

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.OpenUpgrade) == true)
                    {
                        if (dtb.GetHouseUpgrade(id) is HouseUpgradeInfo upgradeInfo &&
                            upgradeInfo.GetLevel(level) is HouseUpgradeLevelInfo levelInfo)
                        {
                            var result = house.ApplyUpgrade(upgradeInfo, level);

                            if (result == HousePurchaseUpgradeResult.Success)
                            {
                                houseInfo.Save();

                                foreach (var member in house.Members.Values)
                                {
                                    if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                                    {
                                        memberChar.DiscoveryClient?.Invoke(c =>
                                        {
                                            c.SendHouseOpenUpgrade(id, level);
                                            c.SendHouseUpdate(house);
                                        });
                                    }
                                }
                            }

                            SendHousePurchaseUpgradeResult(result);
                        }
                        else SendHousePurchaseUpgradeResult(HousePurchaseUpgradeResult.Unknown);
                    }
                    else SendHousePurchaseUpgradeResult(HousePurchaseUpgradeResult.NoPermission);
                });
            });
        }

        private void HandleTakeHouseTaskFromPoolAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var identity = reader.ReadShortString(Encoding.UTF8);

            if (identity is null)
                return;

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.TakeTasks) == true &&
                        house.Tasks.GetValueOrDefault(identity) > 0)
                    {
                        character.CreateHouseQuest(identity).ContinueWith(t =>
                        {
                            if (t.IsCompleted == true &&
                                t.Result is DiscoveryQuest quest)
                            {
                                Invoke(() => Server?.RealmInfo?.Use(r =>
                                {
                                    if (character.AcceptDynamicQuest(quest) == true)
                                    {
                                        house.Tasks[identity] = Math.Max(0, house.Tasks.GetValueOrDefault(identity) - 1);
                                        houseInfo.Save();
                                        BroadcastHouseUpdate();
                                        SendQuestDataUpdate();
                                    }
                                }));
                            }
                        });
                    }
                });
            });
        }

        private void HandleHouseStartDoctrineAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var id = reader.ReadInt32();
            
            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.StartDoctrine) == true)
                    {
                        var dtb = character?.DiscoveryClient.Server.Realm.Database ?? SfaDatabase.Instance;

                        if (dtb.GetHouseDoctrine(id) is HouseDoctrineInfo doctrineInfo &&
                            house.Currency >= doctrineInfo.Price &&
                            character.StartDoctrine(id) is HouseDoctrine doctrine)
                        {
                            house.Currency = house.Currency.SubtractWithoutOverflow(doctrineInfo.Price);
                            houseInfo.Save();

                            foreach (var member in house.Members.Values)
                            {
                                if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                                {
                                    memberChar.DiscoveryClient?.Invoke(c =>
                                    {
                                        memberChar.SyncDoctrines();
                                        c.SendHouseUpdate(house);
                                    });
                                }
                            }
                        }
                    }
                });
            });
        }

        private void HandleHouseSetMemberRankAction(SfReader reader, int systemId, DiscoveryObjectType objectType, int objectId)
        {
            var memberId = reader.ReadInt32();
            var rankId = reader.ReadInt32();

            Invoke(() =>
            {
                Server?.RealmInfo?.Use(r =>
                {
                    if (CurrentCharacter is ServerCharacter character &&
                        character.GetHouseInfo() is SfHouseInfo houseInfo &&
                        houseInfo.House is SfHouse house &&
                        house.GetMember(character) is HouseMember currentCharMember &&
                        character.GetHousePermissions(houseInfo).HasFlag(HouseRankPermission.ChangeRank) == true)
                    {
                        var dtb = (Server?.Realm?.Database ?? SfaDatabase.Instance);
                        var targetMemberChar = Server?.GetCharacter(memberId);
                        var member = memberId < -1 ?
                            house.Members.Values.FirstOrDefault(m => m.Id == memberId) :
                            targetMemberChar?.GetHouseMember(house);


                        if (member is null ||
                            member.Id == currentCharMember.Id)
                        {
                            SendHouseFailChangeMemberRank(memberId, rankId);
                            return;
                        }

                        if (targetMemberChar is null)
                            targetMemberChar = Server.GetCharacter(member);

                        var maxRank = dtb.GetMaxRank();
                        var isMaxRank = rankId == maxRank?.Id;
                        var currentCharRank = dtb.GetRank(currentCharMember.Rank);

                        if (currentCharRank?.Id != maxRank?.Id)
                        {
                            var currentCharRankOrder = currentCharRank?.Order ?? int.MaxValue;
                            var newRankOrder = dtb.GetRank(rankId)?.Order ?? int.MaxValue;
                            var targetMemberRankOrder = dtb.GetRank(member.Rank)?.Order ?? int.MaxValue;

                            if (newRankOrder <= currentCharRankOrder ||
                                targetMemberRankOrder <= currentCharRankOrder)
                            {
                                SendHouseFailChangeMemberRank(memberId, rankId);
                                return;
                            }
                        }

                        member.Rank = rankId;

                        if (isMaxRank)
                        {
                            var ranks = dtb.HouseRanks.Values.OrderBy(v => v.Order).ToArray();

                            if (ranks.Length > 1)
                                currentCharMember.Rank = ranks[1].Id;
                        }

                        houseInfo.Save();

                        BroadcastHouseFullInfo();
                        BroadcastHouseUpdate();
                    }
                });
            });
        }

        public void UpdateHouseMemberInfo()
        {
            Server?.RealmInfo?.Use(r =>
            {
                if (CurrentCharacter is ServerCharacter character &&
                    character.GetHouseInfo() is SfHouseInfo houseInfo &&
                    houseInfo.House is SfHouse house &&
                    house.GetMember(character) is HouseMember member)
                {
                    member.Level = character.Level;
                    member.Name = character.UniqueName;
                    BroadcastHouseMemberInfoChanged();
                }
            });
        }

        public void AddXpToHouse(long xp)
        {
            if (CurrentCharacter?.GetHouseInfo() is SfHouseInfo houseInfo &&
                    houseInfo.House is SfHouse house)
            {
                Server?.RealmInfo.Use(c =>
                {
                    var dtb = Server?.Realm?.Database ?? SfaDatabase.Instance;
                    var newXp = house.Xp.AddWithoutOverflow(xp);
                    var currentLvl = house.Level;
                    var newLvl = dtb.GetHouseLevelInfoForXp(newXp)?.Level ?? 0;

                    if (newXp > house.Xp)
                    {
                        house.Xp = newXp;
                        Invoke(c => c.BroadcastHouseXpChanged());
                    }

                    if (newLvl > house.Level)
                    {
                        house.Level = newLvl;
                        Invoke(c => c.BroadcastHouseUpdate());
                    }

                    houseInfo.Save();
                });
            }
        }

        public void SendHouseInviteResult(HouseUserInviteResult result)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseInviteCharacterResult, writer =>
            {
                writer.WriteByte((byte)result);
            });
        }

        public void BroadcastHouseFullInfo()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.GetHouse() is SfHouse house)
            {
                Server?.RealmInfo?.Use(r =>
                {
                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                            memberChar.DiscoveryClient?.Invoke(c => c.SendHouseFullInfo(house));
                    }
                });
            }
        }

        public void SendHouseFullInfo(SfHouse house)
        {
            try
            {
                var curCharId = 0;
                var curCharGuid = Guid.Empty;
                var curProfileId = Guid.Empty;

                if (CurrentCharacter is ServerCharacter character)
                {
                    curCharId = character.UniqueId;
                    curCharGuid = character.Guid;
                    curProfileId = character.ProfileId;
                }

                Server?.RealmInfo.Use(r =>
                {
                    var dtb = r.Realm?.Database ?? SfaDatabase.Instance;
                    var info = house?.ToJson();

                    if (info is null)
                        return;

                    info["ranks"] = house.Ranks.ToArray()
                        .Where(r => r is not null)
                        .Select(r => new JsonObject
                        {
                            ["id"] = r.Id,
                            ["name"] = dtb.GetRank(r.Id)?.GetName(Client?.Localization) ?? "Rank",
                            ["order"] = r.Order,
                            ["permissions"] = (int)r.Permissions,
                        }).ToJsonArray();

                    info["members"] = house.Members.Values
                        .Where(m => m is not null)
                        .Select(m =>
                        {
                            var id = m.Id;

                            if (m.PlayerId == curProfileId &&
                                m.CharacterId == curCharGuid)
                                id = curCharId;

                            return new JsonObject
                            {
                                ["id"] = id,
                                ["name"] = m.Name,
                                ["currency"] = m.Currency,
                                ["level"] = m.Level,
                                ["rank_id"] = m.Rank,
                            };
                        }).ToJsonArray();

                    var now = DateTime.UtcNow;
                    info["doctrine_cd"] = house.DoctrineCooldown.ToArray()
                        .Select(r => new JsonObject
                        {
                            ["id"] = r.Key,
                            ["minutes"] = Math.Max(0, (r.Value - now).Ticks),
                        }).ToJsonArray();

                    var text = info.ToJsonString(new() { WriteIndented = false });

                    SendGalaxyMessage(DiscoveryServerGalaxyAction.FullHouseInfo, writer =>
                    {
                        writer.WriteShortString(text ?? string.Empty, -1, true, Encoding.UTF8);
                    });
                });
            }
            catch { }
        }

        public void SendHouseXpChanged(long newXp)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseXpChanged, writer =>
            {
                writer.WriteInt64(newXp);
            });
        }

        public void BroadcastHouseXpChanged()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.GetHouse() is SfHouse house)
            {
                Server?.RealmInfo?.Use(r =>
                {
                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseXpChanged(house.Xp));
                    }
                });
            }
        }

        public void SendHouseInvitation(SfHouse house, string inviter)
        {
            if (house is null)
                return;

            SendGalaxyMessage(DiscoveryServerGalaxyAction.InvitationToHouse, writer =>
            {
                writer.WriteShortString(inviter ?? string.Empty, -1, true, Encoding.UTF8); // Inviter
                writer.WriteInt32(house.Id); // House Id
                writer.WriteShortString(house.Name ?? string.Empty, -1, true, Encoding.UTF8); // House Name
                writer.WriteShortString(house.Tag ?? string.Empty, -1, true, Encoding.UTF8); // House Tag
                writer.WriteByte((byte)house.Faction); // House Faction
                writer.WriteInt32(house.Level); // House Lvl
            });
        }

        public void SendHouseInviteResponseResult(int houseId, HouseInviteAcceptResult result)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseInviteResponseResult, writer =>
            {
                writer.WriteInt32(houseId);
                writer.WriteInt32((byte)result);
            });
        }

        public void SendHouseMessageChanged(string message)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseMessageChanged, writer =>
            {
                writer.WriteShortString(message ?? string.Empty, -1, true, Encoding.UTF8);
            });
        }

        public void SendHouseUrlChanged(string url)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseUrlChanged, writer =>
            {
                writer.WriteShortString(url ?? string.Empty, -1, true, Encoding.UTF8);
            });
        }

        public void SendHouseFailChangeMemberRank(int charId, int rankId)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseFailChangeMemberRank, writer =>
            {
                writer.WriteInt32(charId);
                writer.WriteInt32(rankId);
            });
        }

        public void SendHouseCreated(HouseCreationResult result)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.CreatedHouse, writer =>
            {
                writer.WriteByte((byte)result);
            });
        }

        public void SendHouseRankLevelChanged(int rankId, int rankLvl)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseRankLevelChanged, writer =>
            {
                writer.WriteInt32(rankId);
                writer.WriteInt32(rankLvl);
            });
        }

        public void SendHouseRankPermissionChanged(int rankId, int permissionIndex, bool enabled)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseRankPermissionChanged, writer =>
            {
                writer.WriteInt32(rankId);
                writer.WriteInt32((byte)permissionIndex);
                writer.WriteBoolean(enabled);
            });
        }

        public void SendHouseNewMember(int charId, string charName, int rankId)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseNewMember, writer =>
            {
                writer.WriteInt32(charId);
                writer.WriteShortString(charName ?? string.Empty, -1, true, Encoding.UTF8);
                writer.WriteInt32(rankId);
            });
        }

        public void SendHouseDestroyed()
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseDestroyed);
        }

        public void SendHouseCharacterLeft(int charId, CharacterHouseLeftReason reason, int causerId)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.CharacterLeftHouse, writer =>
            {
                writer.WriteInt32(charId);
                writer.WriteInt32((byte)reason);
                writer.WriteInt32(causerId);
            });
        }

        public void SendHouseMemberRankChanged(int charId, int rankId)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseMemberRankChanged, writer =>
            {
                writer.WriteInt32(charId);
                writer.WriteInt32(rankId);
            });
        }

        public void BroadcastHouseMemberRankChanged()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.GetHouse() is SfHouse house &&
                house.GetMember(character) is HouseMember currentMember)
            {
                SendHouseMemberRankChanged(character.UniqueId, currentMember.Rank);

                Server?.RealmInfo?.Use(r =>
                {
                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar && memberChar != character)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseMemberRankChanged(currentMember.Id, currentMember.Rank));
                    }
                });
            }
        }

        public void SendHouseCharacterOnlineStatus(int charId, bool isOnline)
        {
            SendGalaxyMessage(
                isOnline ? DiscoveryServerGalaxyAction.HouseUserSubscribed : DiscoveryServerGalaxyAction.HouseUserUnSubscribed,
                writer =>
            {
                writer.WriteInt32(charId);
            });
        }

        public void BroadcastHouseCharacterOnlineStatus(bool isOnline)
        {
            Server?.RealmInfo?.Use(r =>
            {
                var character = CurrentCharacter;

                if (character is not null &&
                    character.GetHouse() is SfHouse house &&
                    house.GetMember(character) is HouseMember currentMember)
                {
                    SendHouseCharacterOnlineStatus(character.UniqueId, isOnline);

                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar && memberChar != character)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseCharacterOnlineStatus(currentMember.Id, isOnline));
                    }
                }
            });
        }

        public void BroadcastHouseCharacterOnlineStatus(SfHouse house, HouseMember member, bool isOnline)
        {
            if (house is null || member is null)
                return;

            Server?.RealmInfo?.Use(r =>
            {
                var character = CurrentCharacter;

                if (character is not null &&
                    house.GetMember(character) is HouseMember currentMember &&
                    currentMember.Id == member.Id)
                {
                    SendHouseCharacterOnlineStatus(character.UniqueId, isOnline);

                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar && memberChar != character)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseCharacterOnlineStatus(currentMember.Id, isOnline));
                    }
                }
            });
        }

        public void SendHouseUpdate(SfHouse house)
        {
            if (house is null)
                return;

            var dtb = Server?.Realm?.Database ?? SfaDatabase.Instance;
            var tasks = house.Tasks.ToArray();

            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseUpdate, writer =>
            {
                writer.WriteInt64(house.Xp); // Xp
                writer.WriteInt32(house.Level); // Level
                writer.WriteInt32(house.Currency); // Currency
                writer.WriteInt32(house.MaxMembers); // MaxMembers
                writer.WriteInt32(house.TasksPoolSize); // MaxTasks
                writer.WriteInt32(house.MaxCurrency); // MaxCurrency
                writer.WriteShortArray(house.DoctrineAccessLevels ?? new(), (i, w) => w.WriteInt32(i)); // Doctrine Access Levels
                writer.WriteShortArray(tasks, (i, w) => w.WriteShortString(i.Key ?? "", -1, true, Encoding.UTF8)); // Tasks ident
                writer.WriteShortArray(tasks, (i, w) => w.WriteInt32(i.Value)); // Tasks count

            });
        }

        public void BroadcastHouseUpdate()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.GetHouse() is SfHouse house)
            {
                Server?.RealmInfo?.Use(r =>
                {
                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseUpdate(house));
                    }
                });
            }
        }

        public void SendHouseOpenUpgrade(int upgradeId, int upgradeLvl)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseOpenUpgrade, writer =>
            {
                writer.WriteInt32(upgradeId);
                writer.WriteInt32(upgradeLvl);
            });
        }

        public void SendHousePurchaseUpgradeResult(HousePurchaseUpgradeResult result)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HousePurchaseUpgradeResult, writer =>
            {
                writer.WriteInt32((byte)result);
            });
        }

        public void SendHouseMemberInfoChanged(int charId, int newLvl, int newCurrency)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseMemberInfoChanged, writer =>
            {
                writer.WriteInt32(charId);
                writer.WriteInt32(newLvl);
                writer.WriteInt32(newCurrency);
            });
        }

        public void BroadcastHouseMemberInfoChanged()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.GetHouse() is SfHouse house &&
                house.GetMember(character) is HouseMember charMember)
            {
                Server?.RealmInfo?.Use(r =>
                {
                    SendHouseMemberInfoChanged(character.UniqueId, charMember.Level, charMember.Currency);

                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar && memberChar != character)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseMemberInfoChanged(charMember.Id, charMember.Level, charMember.Currency));
                    }
                });
            }
        }

        public void SendHouseCurrencyChanged(int newCurrency)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseCurrencyChanged, writer =>
            {
                writer.WriteInt32(newCurrency);
            });
        }

        public void BroadcastHouseCurrencyChanged()
        {
            if (CurrentCharacter is ServerCharacter character &&
                character.GetHouse() is SfHouse house)
            {
                Server?.RealmInfo?.Use(r =>
                {
                    foreach (var member in house.Members.Values)
                    {
                        if (Server?.GetCharacter(member) is ServerCharacter memberChar)
                            memberChar.DiscoveryClient?.Invoke(
                                c => c.SendHouseCurrencyChanged(house.Currency));
                    }
                });
            }
        }

        public void SendHouseDoctrineCooldownStarted(int doctrineId, TimeSpan duration)
        {
            SendGalaxyMessage(DiscoveryServerGalaxyAction.HouseDoctrineCooldownStarted, writer =>
            {
                writer.WriteInt32(doctrineId);
                writer.WriteInt32((int)duration.Ticks);
            });
        }
    }
}

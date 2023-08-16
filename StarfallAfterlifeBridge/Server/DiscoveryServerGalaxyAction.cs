using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public enum DiscoveryServerGalaxyAction : byte
    {
        StarRenamed = 1,
        InitialWarpedToMothership = 2,
        BlackmarketRemoved = 3,
        BlackmarketSpawned = 4,
        PlanetColonyRace = 5,
        PlanetColonyPopulation = 6,
        PlanetColonyPlanetSupplyStatus = 7,
        PlanetColonyProductStatus = 8,
        MothershipSelection = 9,
        HouseInviteCharacterResult = 41,
        FullHouseInfo = 42,
        HouseXpChanged = 43,
        InvitationToHouse = 44,
        HouseInviteResponseResult = 45,
        HouseMessageChanged = 46,
        HouseUrlChanged = 47,
        HouseFailChangeMemberRank = 48,
        CreatedHouse = 49,
        HouseRankLevelChanged = 50,
        HouseRankPermissionChanged = 51,
        HouseNewMember = 52,
        HouseDestroyed = 53,
        CharacterLeftHouse = 54,
        HouseMemberRankChanged = 55,
        HouseUserSubscribed = 56,
        HouseUserUnSubscribed = 57,
        HouseUpdate = 58,
        HouseOpenUpgrade = 59,
        HousePurchaseUpgradeResult = 60,
        HouseMemberInfoChanged = 61,
        HouseCurrencyChanged = 62,
        HouseDoctrineCooldownStarted = 63,
        QuestDataUpdate = 72,
        QuestCompleteData = 73,
        CharacterCurrencyUpdate = 74,
        FactionEvent = 75,
        FactionProgressInfo = 76,
        CharacterBoosterUpdate = 77,
        SessionDropDone = 78,
        AddOnScreenNotification = 79,
        ShowTalkingHead = 80,
        CharacterXpUpdate = 81,
        UpdateInventory = 82,
        InventoryNewItems = 83
    }
}

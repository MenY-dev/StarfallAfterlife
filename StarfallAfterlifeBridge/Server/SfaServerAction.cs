using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SfaServerAction : byte
    {
        // Empty action
        None = 0,

        // 1..127 - server actions
        Auth = 1,
        GetServerInfo = 2,
        LoadGalaxyMap = 32,

        // 128..255 - game actions
        RegisterPlayer = 128,
        RegisterNewCharacters = 129,
        DeleteCharacter = 130,
        SyncCharacterSelect = 140,
        RequestCharacterDiscoveryData = 141,
        SyncCharacterData = 142,
        RequestCharacterShipData = 143,

        CharacterInventory = 150,

        StartSession = 160,
        EndSession = 161,
        DropSession = 162,
        SyncGalaxySessionData = 163,
        GetFullGalaxySessionData = 164,

        StartBattle = 170,

        SyncProgress = 180,
        AddCharacterCurrencies = 181,
        SyncCharacterCurrencies = 182,
        RequestItemResearch = 182,
        SyncCharacterNewResearch = 183,
        SaveShipsGroup = 184,
        TakeCharactRewardFromQueue = 185,

        RegisterChannel = 190,
        DiscoveryChannel = 192,
        GalacticChannel = 193,
        BattleGroundChannel = 194,
        QuickMatchChannel = 195,
        UserFriendChannel = 196,
        CharacterFriendChannel = 197,

        GlobalChat = 220,
    }
}

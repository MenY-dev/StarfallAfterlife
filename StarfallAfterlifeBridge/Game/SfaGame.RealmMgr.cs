using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static StarfallAfterlife.Bridge.Diagnostics.SfaDebug;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System.Runtime;
using System.Text.Json.Nodes;
using StarfallAfterlife.Bridge.Serialization;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame
    {
        public ChatChannel DeprivedChatChannel { get; protected set; }

        public ChatChannel EclipseChatChannel { get; protected set; }

        public ChatChannel VanguardChatChannel { get; protected set; }

        public FriendChannel CharacterFriendsChannel { get; protected set; }

        public QuickMatchChannel QuickMatchChannel { get; protected set; }

        public CharactPartyChannel CharactPartyChannel { get; protected set; }

        public BattleGroundChannel BattleGroundChannel { get; protected set; }

        public DiscoveryChannel DiscoveryChannel { get; protected set; }

        public GalacticChannel GalacticChannel { get; protected set; }

        protected MgrServer RealmMgrServer { get; set; }

        protected GameChannelManager RealmMgrChannelManager { get; set; }

        protected virtual void RealmInput(HttpListenerContext context, SfaHttpQuery query)
        {
            object response = null;

            Print($"Request ({query})", "realmmgr");

            Profile?.Use(p =>
            {
                switch (query.Function)
                {
                    case "auth":
                    case "authcompletion":
                        response = new JsonObject
                        {
                            ["address"] = SValue.Create(RealmMgrChannelManager.Address.Host),
                            ["port"] = SValue.Create(RealmMgrChannelManager.Address.Port.ToString()),
                            ["temporarypass"] = SValue.Create(GameProfile.TemporaryPass),
                            ["auth"] = SValue.Create(GameProfile.TemporaryPass),
                            ["tutorial_complete"] = SValue.Create(true),
                            ["realmname"] = SValue.Create("NewRealm"),
                            ["userbm"] = SValue.Create(1)
                        };
                        break;

                    case "chatmgr":
                        response = new JsonObject
                        {
                            ["ip"] = SValue.Create(RealmMgrChannelManager.Address.Host),
                            ["port"] = SValue.Create(RealmMgrChannelManager.Address.Port.ToString()),
                        };
                        break;

                    case "getcharacterdata":
                        UpdateProductionPointsIncome(false);
                        if (Enum.TryParse((string)query["data_flags"] ?? string.Empty, out UserDataFlag userDataFlags))
                        {
                            SyncGalaxySessionData();
                            Print($"GetCharacterData(Flags=({userDataFlags}))");
                            response = CreateCharacterDataResponse(userDataFlags);
                        }
                        else
                        {
                            SyncGalaxySessionData();
                            response = CreateCharacterDataResponse();
                        }
                        break;

                    case "discovery_charactgetdata":
                        UpdateProductionPointsIncome(false);
                        UpdateShipsRepairProgress(true);
                        SyncGalaxySessionData();
                        response = new JsonObject
                        {
                            ["result_data"] = SValue.Create(GameProfile?.CurrentCharacter?.CreateDiscoveryCharacterDataResponse())
                        };
                        break;

                    case "get_charact_quests":
                        response = null;
                        break;

                    case "menucurrentdetachment":
                        response = HandleMenuCurrentDetachment(query);
                        break;

                    case "detachmentsave":
                        response = HandleDetachmentSave(query);
                        break;

                    case "detachmentabilitysave":
                        response = HandleDetachmentAbilitySave(query);
                        break;

                    case "galaxymapload":
                        response = CreateGalaxyMapResponse((string)query["hash"] == Realm.GalaxyMapHash);
                        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        break;

                    case "get_charact_stats":
                        response = CreateCharacterStatsResponse();
                        break;

                    case "set_charact_event_checked":
                        response = EmptyMgrResponse;
                        break;

                    case "save_charact_progress_stats":
                        response = EmptyMgrResponse;
                        break;

                    case "ship.save":
                        response = HandleSaveShip(query);
                        break;

                    case "favorite_ship":
                        response = HandleFavoriteShip(query);
                        break;

                    case "disassemble_items":
                        response = HandleDisassembleItems(query);
                        break;

                    case "sellinventory":
                        response = HandleSellInventory(query);
                        break;

                    case "startcrafting":
                        response = HandleStartCrafting(query);
                        break;

                    case "acquirecrafteditem":
                        response = HandleAcquireCraftedItem(query);
                        break;

                    case "acquireallcrafteditems":
                        response = HandleAcquireAllCraftedItems(query);
                        break;

                    case "swap_in_queue":
                        response = HandleSwapCraftingQueue(query);
                        break;

                    case "ship.delete":
                        UpdateShipsRepairProgress(false);
                        response = HandleShipDelete(query);
                        break;

                    case "buy_battle_ground_shop_item":
                        response = HandleBuyBattleGroundShopItem(query);
                        break;

                    case "set_session_reward":
                        UpdateShipsRepairProgress(false);
                        response = HandleConfirmSessionReward(query);
                        break;

                    case "take_charact_reward_from_queue":
                        response = HandleTakeCharactRewardFromQueue(query);
                        break;

                    case "repair_dest_ship":
                        response = HandleRepairShips(query);
                        break;

                    case "rush_crafting_item":
                        response = HandleRushCraftingItem(query);
                        break;

                    default:
                        break;
                }
            });

            
            if (response is JsonNode sr)
            {
                sr = new JsonObject { ["doc"] = sr };
                RealmMgrServer.Send(context, JsonHelpers.ToJsonStringUnbuffered(sr, false));
                sr.AsObject().Clear();
            }
        }

        protected virtual void InitRealmMgr()
        {
            RealmMgrServer ??= new MgrServer(RealmInput);
            RealmMgrChannelManager ??= new GameChannelManager(this);

            DeprivedChatChannel ??= new ChatChannel("Deprived Chat", 1, this);
            EclipseChatChannel ??= new ChatChannel("Eclipse Chat", 2, this);
            VanguardChatChannel ??= new ChatChannel("Vanguard Chat", 3, this);
            CharacterFriendsChannel ??= new FriendChannel("CharacterFriends", 4, this);
            QuickMatchChannel ??= new QuickMatchChannel("QuickMatch", 5, this);
            CharactPartyChannel ??= new CharactPartyChannel("CharactParty", 6, this);
            BattleGroundChannel ??= new BattleGroundChannel("BattleGround", 7, this);
            DiscoveryChannel ??= new DiscoveryChannel("Discovery", 8, this);
            GalacticChannel ??= new GalacticChannel("Galactic", 9, this);

            RealmMgrChannelManager.Add(DeprivedChatChannel);
            RealmMgrChannelManager.Add(EclipseChatChannel);
            RealmMgrChannelManager.Add(VanguardChatChannel);
            RealmMgrChannelManager.Add(CharacterFriendsChannel);
            RealmMgrChannelManager.Add(QuickMatchChannel);
            RealmMgrChannelManager.Add(CharactPartyChannel);
            RealmMgrChannelManager.Add(BattleGroundChannel);
            RealmMgrChannelManager.Add(DiscoveryChannel);
            RealmMgrChannelManager.Add(GalacticChannel);
        }

        public void SyncGalaxySessionData()
        {
            var character = GameProfile?.CurrentCharacter;

            if (character is null)
                return;

            var result = SfaClient?.RequestFullGalaxySesionData().Result;

            if (result is not null)
                SfaClient.SyncSessionData(result);
        }
    }
}

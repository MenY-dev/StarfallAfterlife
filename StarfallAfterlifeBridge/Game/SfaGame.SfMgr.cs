using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static StarfallAfterlife.Bridge.Diagnostics.SfaDebug;

namespace StarfallAfterlife.Bridge.Game
{
    public partial class SfaGame
    {
        public MatchmakerChannel MatchmakerChannel { get; protected set; }

        public ChatConsoleChannel GeneralTextChatChannel { get; protected set; }

        public GameChannel SystemMessagesChannel { get; protected set; }

        public GameChannel UserAnalyticsChannel { get; protected set; }

        public GameChannel UserFriendsChannel { get; protected set; }

        protected MgrServer SfMgrServer { get; set; }

        protected GameChannelManager SfMgrChannelManager { get; set; }

        protected virtual void SfMgrInput(HttpListenerContext context, SfaHttpQuery query)
        {
            object response = null;

            Print($"Request ({query})", "sfmgr");

            Profile?.Use(p =>
            {
                switch (query.Function)
                {
                    case "auth":
                    case "authcompletion":
                        response = new JsonObject
                        {
                            ["address"] = new SValue(SfMgrChannelManager.Address.Host),
                            ["port"] = new SValue(SfMgrChannelManager.Address.Port.ToString()),
                            ["temporarypass"] = new SValue(GameProfile.TemporaryPass),
                            ["auth"] = new SValue(GameProfile.TemporaryPass),
                            ["tutorial_complete"] = new SValue(true),
                            ["realmname"] = new SValue("NewRealm"),
                            ["userbm"] = new SValue(1)
                        };
                        //SfaClient?.SyncCharacterSelectAsync(Profile?.CurrentCharacter).Wait();
                        break;

                    case "getrealms":
                        response = GameProfile.CreateRealmsResponse();
                        break;

                    case "allmyproperty":
                        response = new JsonObject
                        {
                            ["data_result"] = new SValue(GameProfile.CreateAllMyPropertyResponse().ToJsonString())
                        };

                        break;

                    case "charact.edit":
                        response = HandleCharecterEditRequest(query);
                        break;

                    case "charact.select":
                        if (HandleCharacterSelect((int?)query["characterid"] ?? -1))
                            response = GameProfile.CreateCharactSelectResponse(RealmMgrServer.Address);
                        break;

                    case "delete_char_from_realm":
                        response = HandleCharecterDeleteRequest(query);
                        break;

                    case "analyticsregister":
                        response = EmptyMgrResponse;
                        break;

                    case "getspecialfleet":
                        response = EmptyMgrResponse;
                        break;

                    case "getdraftfleets":
                        response = GameProfile.CreateDraftFleetsResponse();
                        break;

                    default:
                        break;
                }
            });

            if (response is JsonNode mr)
            {
                mr = new JsonObject { ["doc"] = mr };
                RealmMgrServer.Send(context, mr.ToJsonString(false));
                mr.Dispose();
            }
            else if (response is System.Text.Json.Nodes.JsonNode sr)
            {
                sr = new System.Text.Json.Nodes.JsonObject { ["doc"] = sr };
                RealmMgrServer.Send(context, Serialization.JsonHelpers.ToJsonStringUnbuffered(sr, false));
            }
            //Print($"Response ({response?.ToJsonString()})", "sfmgr");
        }


        protected bool HandleCharacterSelect(int characterId)
        {
            bool isCharFound = false;

            Profile.Use(p =>
            {
                var character = Profile.GameProfile.DiscoveryModeProfile.Chars.FirstOrDefault(c => c.CurrentId == characterId);

                if (character is not null)
                {
                    //Profile.GameProfile.CurrentCharacter = character;
                    Profile.SelectCharacter(character);
                    isCharFound = true;
                }
            });

            if (isCharFound)
            {
                SfaClient?.SendCharacterSelect().Wait();
                UpdateProductionPointsIncome();
            }


            return isCharFound;
        }

        protected virtual void InitSfMgr()
        {
            SfMgrServer ??= new MgrServer(SfMgrInput);
            SfMgrChannelManager ??= new GameChannelManager();

            MatchmakerChannel ??= new MatchmakerChannel("Matchmaker", 1);
            GeneralTextChatChannel ??= new ChatConsoleChannel("GeneralTextChat", 2, this);
            SystemMessagesChannel ??= new GameChannel("SystemMessages", 3);
            UserAnalyticsChannel ??= new GameChannel("UserAnalytics", 4);
            UserFriendsChannel ??= new GameChannel("UserFriends", 5);

            SfMgrChannelManager.Add(MatchmakerChannel);
            SfMgrChannelManager.Add(GeneralTextChatChannel);
            SfMgrChannelManager.Add(SystemMessagesChannel);
            SfMgrChannelManager.Add(UserAnalyticsChannel);
            SfMgrChannelManager.Add(UserFriendsChannel);
        }
    }
}

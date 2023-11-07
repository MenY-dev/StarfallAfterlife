using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public partial class InstanceManagerServerClient
    {
        protected virtual void GalaxyInput(HttpListenerContext context, SfaHttpQuery query)
        {
            JsonNode response = null;

            SfaDebug.Print($"Request ({query})", "galaxymgr");

            switch (query.Function)
            {
                case "auth":
                case "authcompletion":
                    response = new JsonObject
                    { };
                    break;

                case "discovery_charactgetdata":
                    response = new JsonObject
                    {
                        ["result_data"] = SValue.Create(HandleGetCharacterData(
                            (int?)query["charactid"] ?? -1,
                            (string)query["gamemode"],
                            (int?)query["include_destroyed_ships"] == 1))
                    };
                    break;

                case "getmobfleet":
                    response = new JsonObject
                    {
                        ["mobdata"] = HandleGetMobFleet((string)query["auth"], (int?)query["mobid"] ?? -1)
                    };
                    break;

                case "getmobfleetcustom":
                    response = new JsonObject
                    {
                        ["mobdata"] = HandleGetMobFleetCustom(
                            (Faction?)(byte?)query["faction"] ?? Faction.None,
                            ((string)query["tags"])?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
                            (string)query["auth"]),
                    };
                    break;

                case "getspecialfleet":
                    response = new JsonObject
                    {
                        ["fleetdata"] = HandleGetSpecialFleet((string)query["internalname"], (string)query["auth"])
                    };
                    break;

                case "getdroplist":
                    response = new JsonObject
                    {
                        ["listdata"] = new JsonObject
                        {
                            ["items"] = new JsonArray()
                        }
                    };
                    break;

                case "update_ship_status":
                    UpdateShipStatus(
                        (int?)query["shipid"] ?? -1,
                        (string)query["shipdata"],
                        (string)query["ship_stats"],
                        (string)query["auth"]);
                    break;

                case "update_charact_stats":
                    UpdateCharacterStats(
                        (int?)query["charid"] ?? -1,
                        JsonHelpers.ParseNodeUnbuffered((string)query["stats"]),
                        (string)query["auth"]);
                    break;

                case "battle_results":
                    HandleBattleResults(
                        (string)query["game_mode"],
                        (string)query["results"],
                        (string)query["auth"]);
                    break;
                case "save_ships_group":
                    HandleSaveShipsGroup(
                        (int?)query["charact"] ?? -1,
                        (int?)query["group_num"] ?? -1,
                        JsonHelpers.ParseNodeUnbuffered((string)query["selection_data"]),
                        (string)query["auth"]);
                    break;

                case "add_charact_reward_for_event":
                    response = new JsonObject
                    {
                        ["data_result"] = SValue.Create(HandleAddCharactRewardForEvent((string)query["auth"])),
                    };
                    break;

                default:
                    break;
            }

            SfaDebug.Print($"Response ({response?.ToJsonString()})", "sfmgr");

            response = new JsonObject { ["doc"] = response };
            MgrServer.Send(context, response.ToJsonString(false));
        }

        protected virtual string HandleGetCharacterData(int characterId, string gameMode = null, bool includeDestroyedShips = false)
        {
            string characterData = null;
            var responseWaiter = EventWaiter<CharacterDataResponseEventArgs>
                .Create()
                .Subscribe(e => CharacterDataReceived += e)
                .Unsubscribe(e => CharacterDataReceived -= e)
                .Where((o, e) =>
                {
                    if (e.CharacterId != characterId)
                        return false;

                    characterData = e.Data;
                    return true;
                })
                .Start(20000);

            RequestCharacterData(characterId, gameMode, includeDestroyedShips);
            responseWaiter.Wait();
            return characterData;
        }

        protected virtual JsonNode HandleGetMobFleet(string instanceAuth, int mobId)
        {
            JsonNode mobData = null;
            var responseWaiter = EventWaiter<MobDataResponseEventArgs>
                .Create()
                .Subscribe(e => MobDataReceived += e)
                .Unsubscribe(e => MobDataReceived -= e)
                .Where((o, e) =>
                {
                    if (e.InstanceAuth != instanceAuth ||
                        e.MobId != mobId)
                        return false;

                    mobData = e.Data;
                    return true;
                })
                .Start(20000);

            RequestMobData(mobId, instanceAuth);
            responseWaiter.Wait();
            return mobData;
        }

        private JsonNode HandleGetMobFleetCustom(Faction faction, string[] tags, string auth)
        {
            if (auth is null)
                return null;

            int mobId = (int)faction + auth.GetHashCode();
            tags ??= Array.Empty<string>();

            foreach (var item in tags)
                mobId ^= item?.GetHashCode() ?? 0;

            JsonNode mobData = null;
            var responseWaiter = EventWaiter<MobDataResponseEventArgs>
                .Create()
                .Subscribe(e => MobDataReceived += e)
                .Unsubscribe(e => MobDataReceived -= e)
                .Where((o, e) =>
                {
                    if (e.InstanceAuth != auth ||
                        e.MobId != mobId)
                        return false;

                    mobData = e.Data;
                    return true;
                })
                .Start(20000);

            RequestCustomMobData(mobId, faction, tags, auth);
            responseWaiter.Wait();
            return mobData;
        }

        private JsonNode HandleGetSpecialFleet(string fleetName, string auth)
        {
            JsonNode fleet = null;
            var responseWaiter = EventWaiter<SpecialFleetResponseEventArgs>
                .Create()
                .Subscribe(e => SpecialFleetReceived += e)
                .Unsubscribe(e => SpecialFleetReceived -= e)
                .Where((o, e) =>
                {
                    if (e.FleetName != fleetName ||
                        e.Auth != auth)
                        return false;

                    fleet = e.Data;
                    return true;
                })
                .Start(20000);

            RequestSpecialFleet(fleetName, auth);
            responseWaiter.Wait();
            return new JsonObject { ["ships"] = fleet };
        }


        private void HandleBattleResults(string gameMode, string results, string auth)
        {
            var instance = GetInstance(auth);

            if (instance is null)
                return;

            if (gameMode == "battlegrounds")
            {
                instance.State = InstanceState.Finished;
                instance.StartShutdownTimer(120);
            }
        }

        protected void UpdateCharacterStats(int charId, JsonNode doc, string instanceAuth)
        {
            if (doc is not JsonObject)
                return;

            doc["char_id"] = charId;
            SendInstanceAction(instanceAuth, "update_character_stats", doc.ToJsonString(false));
        }

        private void HandleSaveShipsGroup(int characterId, int groupNumber, JsonNode data, string instanceAuth)
        {
            var formation = data?.DeserializeUnbuffered<ShipsGroup>();

            if (formation is null)
                return;

            formation.Number = groupNumber;

            var doc = new JsonObject
            {
                ["char_id"] = characterId,
                ["group"] = JsonHelpers.ParseNodeUnbuffered(formation).ToJsonString(),
            };

            SendInstanceAction(instanceAuth, "save_ships_group", doc.ToJsonString(false));
        }

        private string HandleAddCharactRewardForEvent(string auth)
        {
            string doc = null;
            var responseWaiter = EventWaiter<RewardForEvenResponseEventArgs>
                .Create()
                .Subscribe(e => RewardForEvenReceived += e)
                .Unsubscribe(e => RewardForEvenReceived -= e)
                .Where((o, e) =>
                {
                    if (e.Auth != auth)
                        return false;

                    doc = e.Reward;
                    return true;
                })
                .Start(20000);

            RequestRewardForEven(auth);
            responseWaiter.Wait();
            return doc ?? "{}";
        }
    }
}

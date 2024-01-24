using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class DiscoveryClient
    {
        public void SyncAcceptNewQuest(int questId)
        {
            Client?.Send(new JsonObject()
            {
                ["new_active_quests"] = new JsonArray(new JsonObject
                {
                    ["id"] = questId,
                })
            }, SfaServerAction.SyncProgress);
        }


        public void SyncQuestProgress(int questId, QuestProgress progress)
        {
            Client?.Send(new JsonObject()
            {
                ["updated_quests"] = new JsonArray(new JsonObject
                {
                    ["id"] = questId,
                    ["progress"] = JsonHelpers.ParseNodeUnbuffered(progress)
                })
            }, SfaServerAction.SyncProgress);
        }

        public void SyncQuestCompleted(int questId)
        {

            Client?.Send(new JsonObject()
            {
                ["new_completed_quests"] = new JsonArray(JsonValue.Create(questId))
            }, SfaServerAction.SyncProgress);
        }

        public void SyncQuestCanceled(int questId)
        {

            Client?.Send(new JsonObject()
            {
                ["new_canceled_quests"] = new JsonArray(JsonValue.Create(questId))
            }, SfaServerAction.SyncProgress);
        }

        public void SyncCharReward(int rewardId)
        {

            Client?.Send(new JsonObject()
            {
                ["new_rewards"] = new JsonArray(JsonValue.Create(rewardId))
            }, SfaServerAction.SyncProgress);
        }

        public void SyncSecretObject(int id)
        {

            Client?.Send(new JsonObject()
            {
                ["new_secrets"] = new JsonArray(JsonValue.Create(id))
            }, SfaServerAction.SyncProgress);
        }

        public void SyncNewSeasonRewards(params int[] newRewards)
        {
            Client?.Send(new JsonObject()
            {
                ["new_season_rewards"] = JsonHelpers.ParseNodeUnbuffered(newRewards),
            }, SfaServerAction.SyncProgress);
        }

        public void SyncNewSeasonProgress(int id, int xp)
        {
            Client?.Send(new JsonObject()
            {
                ["new_season_progress"] = new JsonObject
                {
                    ["id"] = id,
                    ["xp"] = xp,
                },
            }, SfaServerAction.SyncProgress);
        }

        public void SyncExploration(IEnumerable<int> systems, IEnumerable<IGalaxyMapObject> newObjects = null)
        {
            var newSystemsResponse = new JsonArray();
            var newObjectsResponse = new JsonArray();
            var newWarpSystemsResponse = new JsonArray();

            if (systems is not null &&
                CurrentCharacter is ServerCharacter character &&
                character.Fleet is UserFleet fleet)
            {
                if (systems is not null)
                {
                    foreach (var systemId in systems)
                    {
                        SystemHexMap map;

                        if (character.ExplorationProgress.TryGetValue(systemId, out map) == false)
                            character.ExplorationProgress.Add(systemId, map = new SystemHexMap());

                        newSystemsResponse.Add(new JsonObject
                        {
                            ["system_id"] = systemId,
                            ["progress"] = map.ToBase64String(),
                        });
                    }
                }

                if (newObjects is not null)
                {
                    foreach (var item in newObjects)
                    {
                        if (item is null)
                            continue;

                        newObjectsResponse.Add(new JsonObject
                        {
                            ["id"] = item.Id,
                            ["type"] = (int)item.ObjectType,
                        });

                        if (item.ObjectType is GalaxyMapObjectType.QuickTravelGate &&
                            Map?.GetSystem(GalaxyMapObjectType.QuickTravelGate, item.Id) is GalaxyMapStarSystem system)
                        {
                            newWarpSystemsResponse.Add(JsonValue.Create(system.Id));
                        }
                    }
                }
            }

            Client?.Send(new JsonObject()
            {
                ["systems"] = newSystemsResponse,
                ["new_objects"] = newObjectsResponse,
                ["new_warp_systems"] = newWarpSystemsResponse,

            }, SfaServerAction.SyncProgress);
        }

        public void UpdateExploration(int systemId, SystemHex location, int vision)
        {

            Invoke(() =>
            {
                if (CurrentCharacter is ServerCharacter character &&
                    character.Fleet is UserFleet fleet &&
                    character.Progress is CharacterProgress progress &&
                    fleet.System is StarSystem system &&
                    system.Id == systemId)
                {
                    SystemHexMap map;
                    List<StarSystemObject> newObjects = new();
                    bool needSync = false;

                    if (character.ExplorationProgress.TryGetValue(systemId, out map) == false)
                        character.ExplorationProgress.Add(systemId, map = new SystemHexMap());

                    if ((Faction?)system?.Info.Faction is Faction.Deprived or Faction.Eclipse or Faction.Vanguard)
                        vision = 33;

                    if (map.Filling < SystemHexMap.HexesCount)
                    {
                        foreach (var hex in location.GetSpiralEnumerator(vision))
                        {
                            if (hex.GetSize() > 16)
                                continue;

                            int hexIndex = SystemHexMap.HexToArrayIndex(hex);
                            map[hexIndex] = true;
                        }

                        if (map.Filling > (SystemHexMap.HexesCount * 0.98f))
                        {
                            int xp = 10000;
                            map.SetAll(true);
                            needSync |= true;

                            CurrentCharacter?.Events?.Broadcast<IExplorationListener>(l => l.OnSystemExplored(systemId));
                            CurrentCharacter.AddCharacterCurrencies(xp: xp);

                            Invoke(c => c.SendOnScreenNotification(new SfaNotification
                            {
                                Id = "Exploration" + system.Id,
                                Header = "SystemExplored",
                                Text = $"SystemExploredXpReaward",
                                Format = new()
                                {
                                    ["SystemName"] = system.Id.ToString(),
                                    ["ExploreXpReward"] = xp.ToString(),
                                }
                            }));
                        }
                    }

                    foreach (var item in system.GetAllObjects(false))
                    {
                        if (item.Hex.GetDistanceTo(location) > vision)
                            continue;

                        if (progress.AddObject(item.Type, item.Id) == true)
                        {
                            newObjects.Add(item);

                            if (item.Type is DiscoveryObjectType.QuickTravelGate)
                                progress.AddWarpSystem(system.Id);

                            CurrentCharacter?.Events?.Broadcast<IExplorationListener>(l =>
                                l.OnObjectExplored(systemId, item.Type, item.Id));
                        }
                    }

                    needSync |= newObjects.Count > 0;

                    if (needSync == true)
                    {
                        SyncExploration(new[] { systemId }, newObjects);
                    }
                }
            });
        }
    }
}

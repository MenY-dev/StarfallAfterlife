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

        public void SyncExploration(int systemId, IEnumerable<IGalaxyMapObject> newObjects = null)
        {
            if (systemId > -1 &&
                CurrentCharacter is ServerCharacter character &&
                character.Fleet is UserFleet fleet)
            {
                SystemHexMap map;

                if (character.ExplorationProgress.TryGetValue(systemId, out map) == false)
                    character.ExplorationProgress.Add(systemId, map = new SystemHexMap());

                var newObjectsResponse = new JsonArray();

                if (newObjects is not null)
                {
                    foreach (var item in newObjects)
                    {
                        if (item is not null)
                            newObjectsResponse.Add(new JsonObject
                            {
                                ["id"] = item.Id,
                                ["type"] = (byte)item.ObjectType,
                                ["system"] = systemId,
                            });
                    }
                }

                Client?.Send(new JsonObject()
                {
                    ["systems"] = new JsonArray(new JsonObject
                    {
                        ["system_id"] = systemId,
                        ["progress"] = map.ToBase64String(),
                        ["new_objects"] = newObjectsResponse,
                    })
                }, SfaServerAction.SyncProgress);
            }
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
                            map.SetAll(true);
                            needSync |= true;

                            CurrentCharacter?.Events?.Broadcast<IExplorationListener>(l => l.OnSystemExplored(systemId));

                            SendOnScreenNotification(new SfaNotification
                            {
                                Id = "Exploration" + new Random().Next(),
                                Header = "Exploration",
                                Text = $"{system.Info?.Name} Explored!",
                            });
                        }
                    }

                    foreach (var item in system.GetAllObjects(false))
                    {
                        if (item.Hex.GetDistanceTo(location) > vision)
                            continue;

                        if (progress.AddObject(item.Type, item.Id, systemId) == true)
                            newObjects.Add(item);
                    }

                    needSync |= newObjects.Count > 0;

                    if (needSync == true)
                    {
                        SyncExploration(systemId, newObjects);
                    }
                }
            });
        }
    }
}

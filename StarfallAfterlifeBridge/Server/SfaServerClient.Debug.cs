using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient
    {
        public void HandleDebugConsoleInput(string channel, string msg)
        {
            string label = ">";
            SendToChat(channel, "<", msg);

            if (msg.StartsWith("arround ") &&
                msg.Length > 7 &&
                int.TryParse(msg[7..].Trim(), out int jumps))
            {
                if (jumps > 6)
                {
                    jumps = 6;
                    SendToChat(channel, label, "Max radius: 6");
                }

                foreach (var system in Map.GetSystemsArround(CurrentCharacter?.Fleet?.System?.Id ?? -1, jumps))
                {
                    SendToChat(channel, label, $"{system.Key?.Id}: {system.Value}");
                }
            }
            else if (msg.StartsWith("explore ") &&
                msg.Length > 7 &&
                int.TryParse(msg[7..].Trim(), out int exploreRadius))
            {
                if (exploreRadius > 6)
                {
                    exploreRadius = 6;
                    SendToChat(channel, label, "Max radius: 6");
                }

                if (CurrentCharacter is ServerCharacter character &&
                    character.Progress is CharacterProgress progress &&
                    character.Fleet?.System is StarSystem currentSystem)
                {
                    var newSystems = new List<int>();
                    var newObjects = new List<IGalaxyMapObject>();

                    foreach (var system in Map.GetSystemsArround(currentSystem.Id, exploreRadius).Select(i => i.Key))
                    {
                        var systemObjects = system.GetAllObjects().ToList();
                        newObjects.AddRange(systemObjects);
                        newSystems.Add(system.Id);

                        foreach (var item in systemObjects)
                        {
                            progress.AddObject(item.ObjectType, item.Id);

                            if (item.ObjectType is GalaxyMapObjectType.QuickTravelGate)
                                progress.AddWarpSystem(system.Id);
                        }

                        progress.SetSystemProgress(system.Id, new SystemHexMap(true));
                    }

                    character.DiscoveryClient?.SyncExploration(newSystems, newObjects);

                    SendToChat(channel, label, $"Exploration result:");
                    SendToChat(channel, label, $"Systems:{newSystems.Count}");
                    SendToChat(channel, label, $"Objects:{newObjects.Count}");
                    SendToChat(channel, label, "For the exploration to be displayed, re-enter to the galaxy.");
                }
            }
            else if (msg.StartsWith("add sxp ") &&
                msg.Length > 8 &&
                int.TryParse(msg[8..].Trim(), out int shipsXp))
            {
                if (CurrentCharacter is ServerCharacter character &&
                    character.Ships is List<ShipConstructionInfo> ships)
                {
                    var shipsForXp = new Dictionary<int, int>();

                    foreach (var ship in ships)
                        shipsForXp[ship.Id] = shipsXp;

                    character.AddCharacterCurrencies(shipsXp: shipsForXp);
                }
            }
            else if (msg.StartsWith("add xp ") &&
                msg.Length > 7 &&
                int.TryParse(msg[7..].Trim(), out int charXp))
            {
                CurrentCharacter?.AddCharacterCurrencies(xp: charXp);
            }
            else if (msg.StartsWith("add igc ") &&
                msg.Length > 8 &&
                int.TryParse(msg[8..].Trim(), out int charIgc))
            {
                CurrentCharacter?.AddCharacterCurrencies(igc: charIgc);
            }
            else if (msg.StartsWith("add bgc ") &&
                msg.Length > 8 &&
                int.TryParse(msg[8..].Trim(), out int charBgc))
            {
                CurrentCharacter?.AddCharacterCurrencies(bgc: charBgc);
            }
            else if (msg.StartsWith("add party"))
            {
                if (CurrentCharacter is ServerCharacter character)
                {
                    var party = CharacterParty.Create(Server, character.UniqueId);
                    SendToChat(channel, label, $"New party: {party?.Id.ToString() ?? "error"}");
                }
            }
            else if (msg.StartsWith("add item ") &&
                msg.Length > 9 &&
                int.TryParse(msg[9..].Trim(), out int itemId))
            {
                if (CurrentCharacter is ServerCharacter character &&
                    (Server?.Realm?.Database ?? SfaDatabase.Instance)?.GetItem(itemId) is SfaItem item)
                {
                    character.DiscoveryClient?.Invoke(c =>
                    {
                        character.Inventory.AddItem(InventoryItem.Create(item));
                        SendToChat(channel, label, $"{item.Name} added to inventory.");
                    });
                }
            }
            else if (msg.StartsWith("toast ") &&
                    msg.Length > 6 && msg[6..] is string toastMsg)
            {
                SendToChat(channel, label, $"Show Toast: {toastMsg}");
                DiscoveryClient.Invoke(c => c.SendOnScreenNotification(new()
                {
                    Id = "test_toast",
                    Text = toastMsg,
                    LifeTime = 10,
                    Type = SfaNotificationType.Info,
                }));
            }
            else if (msg.StartsWith("jmp ") &&
                msg.Length > 4 &&
                int.TryParse(msg[4..].Trim(), out int system))
            {
                CurrentCharacter?.DiscoveryClient?.SendFleetWarpedMothership();
                CurrentCharacter?.DiscoveryClient?.EnterToStarSystem(system);
                SendToChat(channel, label, "To complete the jump, exit to the main menu, then return to the galaxy.");
            }
            else if (msg.StartsWith("done quest ") &&
                msg.Length > 11 &&
                msg[11..].Trim() is string questInfo)
            {
                if (questInfo == "all")
                {
                    CurrentCharacter.CompleteAllQuests();
                }
                else if (int.TryParse(questInfo, out int questId) == true)
                {
                    CurrentCharacter.CompleteQuest(questId);
                }
            }
        }
    }
}

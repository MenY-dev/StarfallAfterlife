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
                foreach (var system in Map.GetSystemsArround(CurrentCharacter?.Fleet?.System?.Id ?? -1, jumps))
                {
                    SendToChat(channel, label, $"{system.Key?.Id}: {system.Value}");
                }
            }
            else if (msg.StartsWith("explore ") &&
                msg.Length > 7 &&
                int.TryParse(msg[7..].Trim(), out int exploreRadius))
            {
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
            }
        }
    }
}

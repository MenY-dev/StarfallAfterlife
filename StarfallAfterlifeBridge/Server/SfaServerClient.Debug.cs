﻿using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Generators;
using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Discovery.AI;
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
        protected bool DebugExploreAllSystemsUsed { get; set; } = false;

        public void HandleDebugConsoleInput(string channel, string msg)
        {
            string label = ">";
            SendToChat(channel, "<", msg);

            if (msg.StartsWith("arround ") &&
                msg.Length > 7 &&
                int.TryParse(msg[7..].Trim(), out int jumps))
            {
                if (jumps > 10)
                {
                    jumps = 10;
                    SendToChat(channel, label, "Max radius: 10");
                }

                foreach (var system in Map.GetSystemsArround(CurrentCharacter?.Fleet?.System?.Id ?? -1, jumps))
                {
                    SendToChat(channel, label, $"{system.Key?.Id}: {system.Value}");
                }
            }
            else if (msg.StartsWith("explore ") &&
                msg.Length > 7)
            {
                int exploreRadius = 0;

                if ("all".Equals(msg[7..].Trim(), StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    if (DebugExploreAllSystemsUsed == false)
                    {
                        DebugExploreAllSystemsUsed = true;
                        exploreRadius = 1000;
                    }
                    else
                    {
                        exploreRadius = 0;
                        SendToChat(channel, label, "All systems have already been explored!");
                    }
                }
                else if (int.TryParse(msg[7..].Trim(), out exploreRadius) == true &&
                    exploreRadius > 0)
                {
                    if (exploreRadius > 10)
                    {
                        exploreRadius = 10;
                        SendToChat(channel, label, "Max radius: 10");
                    }
                }

                if (exploreRadius > 0 &&
                    CurrentCharacter is ServerCharacter character &&
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
            else if (msg.StartsWith("add hc ") &&
                msg.Length > 7 &&
                int.TryParse(msg[7..].Trim(), out int houseCurrency))
            {
                DiscoveryClient?.Invoke(c =>
                {
                    c.Server?.RealmInfo?.Use(r =>
                    {
                        if (c.CurrentCharacter?.GetHouseInfo() is SfHouseInfo houseInfo &&
                            houseInfo.House is SfHouse house &&
                            house.GetMember(c.CurrentCharacter) is HouseMember member)
                        {
                            house.Currency = house.Currency.AddWithoutOverflow(houseCurrency);
                            member.Currency = member.Currency.AddWithoutOverflow(houseCurrency);
                            houseInfo.Save();
                            c.BroadcastHouseCurrencyChanged();
                            c.BroadcastHouseMemberInfoChanged();
                        }
                    });
                });
            }
            else if (msg.StartsWith("add doctrine ") &&
                msg.Length > 13 &&
                int.TryParse(msg[13..].Trim(), out int doctrineProgress))
            {
                DiscoveryClient?.Invoke(c => c.Server?.RealmInfo?.Use(r =>
                {
                    if (c.CurrentCharacter?.GetHouse() is SfHouse house)
                    {
                        c.CurrentCharacter?.AddDoctrineProgress(878337677, doctrineProgress);
                        c.CurrentCharacter?.AddDoctrineProgress(1474451710, doctrineProgress);
                        c.SendHouseUpdate(house);
                    }
                }));
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
                msg.Length > 9)
            {
                var info = msg[9..].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var itemIdText = info.ElementAtOrDefault(0)?.Trim();
                var countText = info.ElementAtOrDefault(1)?.Trim();

                if (string.IsNullOrWhiteSpace(itemIdText) == false &&
                    int.TryParse(itemIdText, out int itemId) == true &&
                    itemId > 0 &&
                    CurrentCharacter is ServerCharacter character &&
                    (Server?.Realm?.Database ?? SfaDatabase.Instance)?.GetItem(itemId) is SfaItem item)
                {
                    var count = 1;

                    if (string.IsNullOrWhiteSpace(countText) == false &&
                        int.TryParse(countText, out count) == false)
                        count = 0;

                    if (count > 0)
                    {
                        character.DiscoveryClient?.Invoke(c =>
                        {
                            character.Inventory?.AddItem(InventoryItem.Create(item, count));
                            SendToChat(channel, label, $"{item.Name}({count}) added to inventory.");
                        });
                    }
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
            else if ("m".Equals(msg?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                DiscoveryClient.Invoke(c =>
                {
                    if (CurrentCharacter?.Fleet is UserFleet charFleet &&
                        charFleet.System is StarSystem starSystem)
                    {
                        var mob = new GalaxyPatrolMobGenerator(Server?.Realm)
                        {
                            Faction = Faction.Pyramid,
                            Level = 10,
                        }.Build();

                        if (mob is null)
                        {
                            SendToChat(channel, label, "Mob creation error");
                            return;
                        }

                        Server.UseDynamicMobs(dtb => dtb.Add(mob));

                        var fleet = new DiscoveryAiFleet
                        {
                            Id = mob.Id,
                            Faction = mob.Faction,
                            Name = "nebtrashlvl2",
                            MobId = -mob.Id,
                            Level = mob.Level,
                            FactionGroup = 1,
                            BaseVision = 0,
                            AgroVision = -100,
                            BaseSpeed = 5,
                            UseRespawn = false,
                            Hull = mob.Ships?.ElementAtOrDefault(mob.MainShipIndex)?.Data?.Hull ??
                                   mob.Ships?.FirstOrDefault()?.Data?.Hull ?? 0,
                        };

                        fleet.SetLocation(SystemHexMap.HexToSystemPoint(
                            starSystem.GetNearestSafeHex(charFleet, charFleet.Hex, true)));

                        fleet.SetAI(new GalaxyPatrollingAI()
                        {
                            Archetype = AIArchetype.Miner,
                        });

                        Galaxy.BeginPreUpdateAction(_ =>
                        {
                            starSystem.AddFleet(fleet);
                        });

                        SendToChat(channel, label, $"Spawn {fleet.Name}!");
                    }
                });
            }

            else if (msg.StartsWith("mj ") &&
                msg.Length > 3 &&
                int.TryParse(msg[3..].Trim(), out int mobJumpSystem))
            {
                if (CurrentCharacter?.Fleet is UserFleet charFleet &&
                    charFleet.System is StarSystem starSystem &&
                    starSystem.Galaxy?.Map?.GetSystem(mobJumpSystem) is not null)
                {
                    var testMobId = new FleetIdInfo { Type = FleetType.DynamicMob, LocalId = 321789 }.ToId();

                    Galaxy.BeginPreUpdateAction(_ =>
                    {
                        if (starSystem.Fleets.FirstOrDefault(f => f.Id == testMobId) is DiscoveryFleet mob &&
                            mob.AI is GalaxyPatrollingAI ai)
                        {
                            ai.StateMachine?.StartStateByName("change_system", mobJumpSystem);
                            DiscoveryClient?.Invoke(c => SendToChat(channel, label, $"Move {mob.Name} to {mobJumpSystem}"));
                        }
                    });
                }
            }
        }
    }
}

using StarfallAfterlife.Bridge.Database;
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

namespace StarfallAfterlife.Bridge.Server
{
    public partial class SfaServerClient
    {
        protected bool DebugExploreAllSystemsUsed { get; set; } = false;

        public ChatConsole Console { get; set; }

        public void InitConsole()
        {
            var newConsole = new ChatConsole(this);

            newConsole.AddHandler("", context =>
            {
                context.Print($"Unknown command: {context.Input}");
            });

            newConsole.AddHandler("arround", context =>
            {
                if (context.Parce<int>() is int jumps)
                {
                    if (jumps > 10)
                    {
                        jumps = 10;
                        context.Print("Max radius: 10");
                    }

                    foreach (var system in Map.GetSystemsArround(CurrentCharacter?.Fleet?.System?.Id ?? -1, jumps))
                    {
                        context.Print($"{system.Key?.Id}: {system.Value}");
                    }
                }
                else context.PrintParametersError();
            });
            
            newConsole.AddHandler("explore", context =>
            {
                int exploreRadius = 0;

                if ("all".Equals(context.Input?.Trim(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (DebugExploreAllSystemsUsed == false)
                    {
                        DebugExploreAllSystemsUsed = true;
                        exploreRadius = 1000;
                    }
                    else
                    {
                        exploreRadius = 0;
                        context.Print("All systems have already been explored!");
                    }
                }
                else if (context.Parce<int>() is int radius &&
                    radius > 0)
                {
                    exploreRadius = radius;

                    if (exploreRadius > 10)
                    {
                        exploreRadius = 10;
                        context.Print("Max radius: 10");
                    }
                }
                else
                {
                    context.PrintParametersError();
                    return;
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

                    context.Print($"Exploration result:");
                    context.Print($"Systems:{newSystems.Count}");
                    context.Print($"Objects:{newObjects.Count}");
                    context.Print("For the exploration to be displayed, re-enter to the galaxy.");
                }
            });

            newConsole.AddHandler("add sxp", context =>
            {
                if (context.Parce<int>() is int shipsXp)
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
                else context.PrintParametersError();
            });

            newConsole.AddHandler("add xp", context =>
            {
                if (context.Parce<int>() is int charXp)
                {
                    CurrentCharacter?.AddCharacterCurrencies(xp: charXp);
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("add igc", context =>
            {
                if (context.Parce<int>() is int charIgc)
                {
                    CurrentCharacter?.AddCharacterCurrencies(igc: charIgc);
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("add bgc", context =>
            {
                if (context.Parce<int>() is int charBgc)
                {
                    CurrentCharacter?.AddCharacterCurrencies(bgc: charBgc);
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("add hc", context =>
            {
                if (context.Parce<int>() is int houseCurrency)
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
                                c.BroadcastHouseUpdate();
                                c.BroadcastHouseCurrencyChanged();
                                c.BroadcastHouseMemberInfoChanged();
                            }
                        });
                    });
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("add doctrine", context =>
            {
                if (context.Parce<int>() is int doctrineProgress)
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
                else context.PrintParametersError();
            });

            newConsole.AddHandler("add item", context =>
            {
                var info = context.Input?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var itemIdText = info?.ElementAtOrDefault(0)?.Trim();
                var countText = info?.ElementAtOrDefault(1)?.Trim();

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
                            context.Print($"{item.Name}({count}) added to inventory.");
                        });
                    }
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("toast", context =>
            {
                if (context.Input is string toastMsg)
                {
                    context.Print($"Show Toast: {toastMsg}");
                    DiscoveryClient.Invoke(c => c.SendOnScreenNotification(new()
                    {
                        Id = "test_toast",
                        Text = toastMsg,
                        LifeTime = 10,
                        Type = SfaNotificationType.Info,
                    }));
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("jmp", context =>
            {
                if (context.Parce<int>() is int system &&
                    CurrentCharacter is ServerCharacter character &&
                    character.Fleet is UserFleet fleet)
                {
                    character.DiscoveryClient?.SendFleetWarpedMothership();
                    character.DiscoveryClient?.EnterToStarSystem(system);
                    context.Print("To complete the jump, exit to the main menu, then return to the galaxy.");
                }
                else context.PrintParametersError();
            });

            newConsole.AddHandler("done quest", context =>
            {
                if (context.Input is string questInfo)
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
                else context.PrintParametersError();
            });

            Console = newConsole; 
        }

        public void HandleDebugConsoleInput(string channel, string msg)
        {
            if (Console is null)
                InitConsole();

            SendToChat(channel, "<", msg);
            Console?.Exec(msg, channel);
        }
    }
}

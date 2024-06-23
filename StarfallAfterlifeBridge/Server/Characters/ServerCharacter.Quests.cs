using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Inventory;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public partial class ServerCharacter
    {
        public void UpdateDailyQuests()
        {
            lock (this)
            {
                try
                {
                    var lastAcceptParam = "last_daily_quest_accept";
                    var lastAccept = (DateTime?)GetRealmParam(lastAcceptParam) ?? DateTime.MinValue;
                    var now = DateTime.UtcNow;

                    if (lastAccept.DayOfYear == now.DayOfYear ||
                        ActiveQuests.Any(q => q?.Info?.Type == QuestType.Daily) == true)
                        return;

                    if ((Realm?.Database ?? SfaDatabase.Instance) is SfaDatabase dtb)
                    {
                        var accesLvl = AccessLevel;
                        var logicId = "daily_quest_logic" + accesLvl;
                        var allLogics = dtb.QuestsLogics.Values
                            .Where(l => l.Type == QuestType.Daily)
                            .Where(l => logicId.Equals(l.UniqueLogicIdentifier, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        if (allLogics.Count < 1)
                            return;

                        var rnd = new Random128();
                        var logic = allLogics[rnd.Next(0, allLogics.Count)];

                        DiscoveryClient?.Server?.UseQuestGenerator(gen =>
                        {
                            try
                            {
                                var quest = gen.GenerateDailyTask(logic);

                                if (quest is null)
                                    return;

                                var revardType = rnd.Next() % 3;
                                var revardItems = new List<QuestRevardItemInfo>();

                                if (revardType != 1)
                                {
                                    var availableItems = (
                                        from bp in dtb.Blueprints.Values
                                        where bp.MinLvl <= accesLvl && bp.IsAvailableForTrading == true
                                        where bp.IsDefective == false && bp.Faction is Faction.None or Faction.Other
                                        select bp).ToList();

                                    if (availableItems.Count > 0)
                                    {
                                        var item = availableItems[rnd.Next(0, availableItems.Count)];
                                        revardItems.Add(new() { Count = rnd.Next(1,3), Id = item.Id });
                                    }
                                }

                                quest.Reward = quest.Reward.Combine(new()
                                {
                                    IGC = revardType > 0 ? accesLvl * 14000 : 0,
                                    Xp = revardType < 2 ? accesLvl * 15000 : 0,
                                    Items = revardItems,
                                });

                                SetRealmParam(lastAcceptParam, now);

                                DiscoveryClient?.Invoke(c => c.Server?.RealmInfo?.Use(r =>
                                {
                                    if (AcceptDynamicQuest(quest, false) == true)
                                        c.SendQuestDataUpdate();
                                }));
                            }
                            catch { }
                        });
                    }
                }
                catch { }
            }
        }

        public void UpdateQuestLines()
        {
            if (Realm is SfaRealm realm &&
                realm.Database is SfaDatabase database &&
                realm.QuestsDatabase is DiscoveryQuestsDatabase questsDatabase)
            {
                foreach (var questId in questsDatabase.QuestLines?
                    .Where(l => Database.GetQuestLine(l?.Id ?? -1)?.Faction == Faction)
                    .SelectMany(l => l.GetNewQuests(this) ?? new()))
                    AcceptQuest(questId);

                foreach (var quest in questsDatabase.LevelingQuests?.Values
                    .Where(
                        q => q?.ObjectFaction == Faction &&
                        q.Level <= Level &&
                        Progress?.CompletedQuests?.Contains(q.Id) != true))
                    AcceptQuest(quest.Id);
            }
        }

        public void AddXpToSeasons(int newXp)
        {
            lock (_characterLocker)
            {
                if (Realm.Seasons is WeeklyQuestsInfo seasonsInfo &&
                    seasonsInfo.Seasons.FirstOrDefault(s => s.IsActive > 0) is WeeklyQuest currentSeason &&
                    Progress is CharacterProgress progress)
                {
                    var currentProgress = progress.SeasonsProgress ??= new();
                    var currentRewards = progress.SeasonsRewards ??= new();
                    var currentXp = progress.SeasonsProgress?.GetValueOrDefault(currentSeason.Id) ?? 0;
                    var totalXp = Math.Max(0, currentXp.AddWithoutOverflow(newXp));
                    var completedStages = seasonsInfo.GetStages(currentSeason.Id, totalXp);
                    var newRewards = seasonsInfo.Rewards?.Where(r =>
                                     completedStages.Any(s => s.Id == r.Stage) == true &&
                                     currentRewards.Contains(r.Id) == false).ToList() ?? new();

                    progress.AddSeasonProgress(currentSeason.Id, newXp);

                    foreach (var item in newRewards)
                        progress.AddSeasonReward(item.Id);

                    DiscoveryClient.Invoke(c =>
                    {
                        c.SyncNewSeasonProgress(currentSeason.Id, newXp);
                        c.SyncNewSeasonRewards(newRewards.Select(r => r.Id).ToArray());
                    });

                    var dst = CargoTransactionEndPoint.CreateForCharacterInventory(this);
                    var items = newRewards.Select(r => r.Type switch
                    {
                        WeeklyRewardType.ShipProject => new InventoryItem()
                        {
                            Id = r.Data?.ShipProjectId ?? 0,
                            Count = r.Count,
                            Type = InventoryItemType.ShipProject
                        },
                        WeeklyRewardType.UniqueEquipment => new InventoryItem()
                        {
                            Id = r.Data?.EquipmentId ?? 0,
                            Count = r.Count,
                            Type = InventoryItemType.Equipment
                        },
                        _ => InventoryItem.Empty
                    }).Where(i => i.IsEmpty == false);

                    foreach (var item in items)
                        DiscoveryClient.Invoke(() => dst.Receive(item, item.Count));
                }
            }
        }
    }
}

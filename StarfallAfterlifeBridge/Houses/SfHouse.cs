using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace StarfallAfterlife.Bridge.Houses
{
    public class SfHouse
    {
        public int Id { get; set; }

        public Guid Guid { get; set; } = Guid.Empty;

        public string Name { get; set; }

        public string Tag { get; set; }

        public Faction Faction { get; set; } = Faction.None;

        public long Xp { get; set; }

        public int Level { get; set; }

        public string Link { get; set; }

        public string MessageOfTheDay { get; set; }

        public int Currency { get; set; }

        public int MaxMembers { get; set; }

        public int TasksPoolSize { get; set; }

        public DateTime TasksPoolUpdateTime { get; set; }

        public int MaxCurrency { get; set; }

        public HashSet<int> DoctrineAccessLevels { get; } = new();

        public List<HouseRank> Ranks { get; } = new();

        public Dictionary<int, HouseMember> Members { get; } = new();

        public List<KeyValuePair<int, int>> Upgrades { get; } = new();

        public List<HouseDoctrine> Doctrines { get; } = new();

        public Dictionary<int, DateTime> DoctrineCooldown { get; } = new();

        public Dictionary<string, int> Tasks { get; } = new();

        public JsonNode ToJson()
        {
            return new JsonObject
            {
                ["id"] = Id,
                ["guid"] = Guid,
                ["name"] = Name,
                ["tag"] = Tag,
                ["faction"] = (int)Faction,
                ["xp"] = Xp,
                ["level"] = Level,
                ["link"] = Link ?? "",
                ["motd"] = MessageOfTheDay ?? "",
                ["task_pool_size"] = TasksPoolSize,
                ["task_pool_update_time"] = TasksPoolUpdateTime,
                ["currency"] = Currency,
                ["max_currency"] = MaxCurrency,
                ["max_members"] = MaxMembers,
                ["doctrine_levels"] = JsonSerializer.SerializeToNode(DoctrineAccessLevels),
                ["ranks"] = JsonSerializer.SerializeToNode(Ranks),
                ["upgrades"] = Upgrades.Select(u => new JsonObject
                {
                    ["upgrade_id"] = u.Key,
                    ["upgrade_level"] = u.Value,
                }).ToJsonArray(),
                ["doctrines"] = JsonSerializer.SerializeToNode(Doctrines),
                ["doctrine_cd"] = DoctrineCooldown.Select(u => new JsonObject
                {
                    ["id"] = u.Key,
                    ["end_time"] = u.Value,
                }).ToJsonArray(),
                ["tasks"] = Tasks.Select(u => new JsonObject
                {
                    ["ident"] = u.Key,
                    ["count"] = u.Value,
                }).ToJsonArray(),
                ["members"] = JsonSerializer.SerializeToNode(Members.Values),
            };
        }

        public bool LoadFromJson(JsonNode doc)
        {
            DoctrineAccessLevels.Clear();
            Ranks.Clear();
            Tasks.Clear();
            Members.Clear();
            Upgrades.Clear();
            DoctrineCooldown.Clear();
            Doctrines.Clear();

            if (doc is not JsonObject)
                return false;

            try
            {
                Id = (int?)doc["id"] ?? 0;
                Guid = (Guid?)doc["guid"] ?? Guid.Empty;
                Name = (string)doc["name"];
                Tag = (string)doc["tag"];
                Faction = (Faction?)(int?)doc["faction"] ?? Faction.None;
                Xp = (long?)doc["xp"] ?? 0;
                Level = (int?)doc["level"] ?? 0;
                Link = (string)doc["link"];
                MessageOfTheDay = (string)doc["motd"];
                TasksPoolSize = (int?)doc["task_pool_size"] ?? 0;
                TasksPoolUpdateTime = (DateTime?)doc["task_pool_update_time"] ?? default;
                Currency = (int?)doc["currency"] ?? 0;
                MaxCurrency = (int?)doc["max_currency"] ?? 0;
                MaxMembers = (int?)doc["max_members"] ?? 0;

                foreach (var item in doc["doctrine_levels"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        if ((int?)item is int lvl)
                            DoctrineAccessLevels.Add(lvl);
                    }
                    catch { }
                }

                foreach (var item in doc["ranks"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        if (item?.DeserializeUnbuffered<HouseRank>() is HouseRank rank)
                            Ranks.Add(rank);
                    }
                    catch { }
                }

                foreach (var item in doc["members"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        var member = JsonHelpers.DeserializeUnbuffered<HouseMember>(item);

                        if (member is not null)
                            Members[member.Id] = member;
                    }
                    catch { }
                }

                foreach (var item in doc["upgrades"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        if (item is JsonObject upgrade &&
                            (int?)upgrade["upgrade_id"] is int id)
                            Upgrades.Add(new(id, (int?)upgrade["upgrade_level"] ?? 0));
                    }
                    catch { }
                }

                foreach (var item in doc["doctrine_cd"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        if (item is JsonObject doctrine &&
                            (int?)doctrine["id"] is int id)
                            DoctrineCooldown[id] = (DateTime?)doctrine["end_time"] ?? default;
                    }
                    catch { }
                }

                foreach (var item in doc["tasks"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        if (item is JsonObject task &&
                            (string)task["ident"] is string ident)
                            Tasks[ident] = (int?)task["count"] ?? 0;
                    }
                    catch { }
                }

                foreach (var item in doc["doctrines"]?.AsArraySelf() ?? new())
                {
                    try
                    {
                        if (item?.DeserializeUnbuffered<HouseDoctrine>() is HouseDoctrine doctrine)
                            Doctrines.Add(doctrine);
                    }
                    catch { }
                }

                return true;
            }
            catch { }

            return false;
        }

        public void AddMember(HouseMember houseMember)
        {
            if (houseMember is null)
                return;

            var id = -2;

            for (int i = 0; i < Members.Count + 1; i++)
            {
                if (Members.Values.All(m => m?.Id != id) == true)
                    break;

                id--;
            }

            houseMember.Id = id;
            Members[id] = houseMember;
        }

        public HouseMember RemoveMember(Guid playerId, Guid charId)
        {
            var member = Members.FirstOrDefault(
                m => m.Value?.PlayerId == playerId &&
                m.Value.CharacterId == charId);

            Members.Remove(member.Key);
            return member.Value;
        }

        public bool GetUpgradeState(int id, int lvl) =>
            Upgrades.Any(u => u.Key == id && u.Value == lvl);

        public void SetUpgradeState(int id, int lvl, bool state)
        {
            if (state == false)
            {
                Upgrades.RemoveAll(u => u.Key == id && u.Value == lvl);
                return;
            }

            if (GetUpgradeState(id, lvl) == true)
                return;

            Upgrades.Add(new(id, lvl));
        }

        public HousePurchaseUpgradeResult ApplyUpgrade(HouseUpgradeInfo upgrade, int level)
        {
            if (GetUpgradeState(upgrade.Id, level) == true)
                return HousePurchaseUpgradeResult.AlreadyOpened;

            if (upgrade.GetLevel(level) is HouseUpgradeLevelInfo levelInfo)
            {
                if (Level < levelInfo.RequiredLevel)
                    return HousePurchaseUpgradeResult.RequirementsNotMet;

                var price = Math.Max(0, levelInfo.Price);

                if (Currency < price)
                    return HousePurchaseUpgradeResult.NotEnoughCurrency;

                SetUpgradeState(upgrade.Id, level, true);
                Currency -= price;

                int upgradeParam = 0;

                try
                {
                    upgradeParam = (int?)levelInfo.Params?["value"] ?? 0;
                }
                catch { }

                switch (upgrade.Type)
                {
                    case 1: MaxMembers += upgradeParam; break;
                    case 2: IncreaseTasksPoolSize(upgradeParam); break;
                    case 3: MaxCurrency += upgradeParam; break;
                    case 8: DoctrineAccessLevels.Add(upgradeParam); break;
                }

                return HousePurchaseUpgradeResult.Success;
            }

            return HousePurchaseUpgradeResult.Unknown;
        }

        public bool PickHouseTask(string indent)
        {
            if (indent is null)
                return false;

            var count = Tasks.GetValueOrDefault(indent);

            if (count < 1)
                return false;

            Tasks[indent] = count - 1;
            return true;
        }

        public void IncreaseTasksPoolSize(int count)
        {
            if (count < 1)
                return;

            TasksPoolSize += count;

            foreach (var task in Tasks.ToArray())
                Tasks[task.Key] = task.Value + count;
        }

        public bool UpdateTasksPool()
        {
            static int GetWeek(DateTime time) => CultureInfo.InvariantCulture.Calendar
                .GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Sunday);

            var currentTime = DateTime.UtcNow;

            if (GetWeek(currentTime) == GetWeek(TasksPoolUpdateTime) &&
                currentTime.Year == TasksPoolUpdateTime.Year)
                return false;

            TasksPoolUpdateTime = currentTime;
            Tasks["house_task_kill_mob"] = TasksPoolSize;
            Tasks["house_task_scan_unknown_planet"] = TasksPoolSize;
            Tasks["house_task_kill_boss"] = TasksPoolSize;

            return true;
        }

        public HouseDoctrine GetDoctrine(int doctrineId)
        {
            UpdateDoctrines();
            return Doctrines.FirstOrDefault(d => d?.Info.Id == doctrineId);
        }

        public bool AddDoctrine(HouseDoctrine doctrine)
        {
            UpdateDoctrines();

            if (doctrine is null)
                return false;

            RemoveDoctrine(doctrine.Info.Id);
            Doctrines.Add(doctrine);
            DoctrineCooldown[doctrine.Info.Id] = doctrine.EndTime;
            return true;
        }

        public void RemoveDoctrine(int doctrineId)
        {
            UpdateDoctrines();
            Doctrines.RemoveAll(d => d.Info.Id == doctrineId);
            DoctrineCooldown.Remove(doctrineId);
        }

        public void UpdateDoctrines()
        {
            var toRemove = new List<HouseDoctrine>();

            foreach (var doctrine in Doctrines)
            {
                if (doctrine is null)
                    continue;

                if (doctrine.IsCompleted || doctrine.IsEndOfTime)
                {
                    toRemove.Add(doctrine);
                    AddEffectToMembers(
                        doctrine.Info.Effect,
                        doctrine.Info.EffectDuration);
                }
            }

            foreach (var item in toRemove)
            {
                Doctrines.Remove(item);
                DoctrineCooldown.Remove(item.Info.Id);
            }
        }

        public void AddEffectToMembers(int effectId, double duration)
        {
            foreach (var member in Members.Values)
            {
                var memberEffects = member.Effects ??= new();
                var effect = memberEffects.FirstOrDefault(e => e?.Id == effectId);
                var currentTime = DateTime.UtcNow;

                if (effect is null)
                {
                    effect = new() { Id = effectId };
                    memberEffects.Add(effect);
                }

                if (effect.EndTime < currentTime)
                    effect.EndTime = currentTime;

                effect.EndTime += TimeSpan.FromHours(duration);
            }
        }
    }
}

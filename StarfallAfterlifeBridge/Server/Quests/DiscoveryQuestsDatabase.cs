﻿using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DiscoveryQuestsDatabase : SfaObject
    {
        public string Hash { get; set; }

        public List<DiscoveryQuestLine> QuestLines { get; set; } = new();

        public Dictionary<int, DiscoveryQuest> Quests { get; set; } = new();

        public Dictionary<int, DiscoveryQuest> LevelingQuests { get; set; } = new();

        protected Dictionary<ulong, HashSet<DiscoveryQuest>> Bindings { get; set; } = new();

        public DiscoveryQuest GetQuest(int id)
        {
            if (Quests.TryGetValue(id, out DiscoveryQuest quest) == true ||
                LevelingQuests.TryGetValue(id, out quest) == true)
                return quest;

            return null;
        }
        
        public IEnumerable<DiscoveryQuest> GetQuests(QuestType questType, byte objectType, int objectId)
        {

            var hash = CreateBindingHash(questType, objectType, objectId);

            if (Bindings.TryGetValue(hash, out HashSet<DiscoveryQuest> quests) == true)
                return quests;

            return Enumerable.Empty<DiscoveryQuest>();
        }

        public IEnumerable<DiscoveryQuest> GetTaskBoardQuests(byte objectType, int objectId)
        {
            var outQuests = Enumerable.Empty<DiscoveryQuest>();
            var hash = CreateBindingHash(QuestType.Task, objectType, objectId);

            if (Bindings.TryGetValue(hash, out HashSet<DiscoveryQuest> quests) == true)
                outQuests = outQuests.Concat(quests);

            hash = CreateBindingHash(QuestType.MainQuestLine, objectType, objectId);

            if (Bindings.TryGetValue(hash, out quests) == true)
                outQuests = outQuests.Concat(quests);

            hash = CreateBindingHash(QuestType.UniqueQuestLine, objectType, objectId);

            if (Bindings.TryGetValue(hash, out quests) == true)
                outQuests = outQuests.Concat(quests);

            return outQuests;
        }

        public void AddQuest(DiscoveryQuest quest)
        {
            if (quest?.Id is int questId &&
                Quests.TryAdd(questId, quest) == true)
            {
                var hash = CreateBindingHash(quest);

                if (Bindings.TryGetValue(hash, out HashSet<DiscoveryQuest> quests) == true)
                    quests.Add(quest);
                else
                    Bindings.Add(hash, new() { quest });
            }
        }

        public bool RemoveQuest(DiscoveryQuest quest)
        {
            if (quest?.Id is int questId &&
                Quests.TryGetValue(quest.Id, out DiscoveryQuest outQuest) == true &&
                outQuest == quest)
            {
                if (Quests.Remove(questId) == true)
                {
                    RemoveFromBindings(quest);
                    return true;
                }
            }

            return false;
        }


        public void AddQuestLine(DiscoveryQuestLine questLine)
        {
            if (questLine is null ||
                QuestLines.Contains(questLine) ||
                QuestLines.Any(q => q.Id == questLine.Id))
                return;

            QuestLines.Add(questLine);
        }

        public DiscoveryQuest GetLevelingQuestById(int id)
        {
            if (LevelingQuests.TryGetValue(id, out DiscoveryQuest quest) == true)
                return quest;

            return null;
        }

        public DiscoveryQuest[] GetLevelingQuest(int level) =>
            LevelingQuests.Values.Where(q => q.Level == level).ToArray() ?? Array.Empty<DiscoveryQuest>();

        public void AddLevelingQuest(DiscoveryQuest quest)
        {
            if (quest?.Id is int questId)
                LevelingQuests.TryAdd(questId, quest);
        }

        public bool RemoveLevelingQuest(DiscoveryQuest quest)
        {
            if (quest?.Id is int questId &&
                LevelingQuests.TryGetValue(quest.Id, out DiscoveryQuest outQuest) == true &&
                outQuest == quest)
                return Quests.Remove(questId);

            return false;
        }

        protected void RemoveFromBindings(DiscoveryQuest quest)
        {
            var hash = CreateBindingHash(quest);

            if (Bindings.TryGetValue(hash, out HashSet<DiscoveryQuest> quests))
            {
                quests.Remove(quest);

                if (quests.Count < 1)
                    Bindings.Remove(hash);
            }
        }

        private static ulong CreateBindingHash(DiscoveryQuest quest)
        {
            if (quest is null)
                return ulong.MaxValue;

            return CreateBindingHash(quest.Type, (byte)quest.ObjectType, quest.ObjectId);
        }

        private static ulong CreateBindingHash(QuestType questType, byte objectType, int objectId)
        {
            return (ulong)questType << 48 | (ulong)objectType << 32 | (uint)objectId;
        }

        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is null)
                return;

            Hash = (string)doc["hash"];
            Quests.Clear();

            if (doc["quests"]?.AsArray() is JsonArray quests)
            {
                foreach (var quest in quests)
                    AddQuest(quest.DeserializeUnbuffered<DiscoveryQuest>());
            }

            if (doc["leveling_quests"]?.AsArray() is JsonArray lvlQuests)
            {
                foreach (var quest in lvlQuests)
                    AddLevelingQuest(quest.DeserializeUnbuffered<DiscoveryQuest>());
            }

            foreach (var item in doc["quest_lines"]?.AsArraySelf() ?? new())
            {
                if (item is null)
                    continue;
                var questLine = new DiscoveryQuestLine();
                questLine.LoadFromJson(item);
                QuestLines.Add(questLine);
            }
        }

        public override JsonNode ToJson()
        {
            var doc = base.ToJson();

            if (doc is not JsonObject)
                doc = new JsonObject();

            var quests = new JsonArray();

            foreach (var quest in Quests.Values)
                quests.Add(JsonHelpers.ParseNodeUnbuffered(quest));

            doc["hash"] = Hash;
            doc["quests"] = quests;
            doc["quest_lines"] = new JsonArray(QuestLines.Select(l => l.ToJson()).ToArray());
            doc["leveling_quests"] = new JsonArray(LevelingQuests.Values.Select(l => JsonHelpers.ParseNodeUnbuffered(l)).ToArray());

            return doc;
        }
    }
}

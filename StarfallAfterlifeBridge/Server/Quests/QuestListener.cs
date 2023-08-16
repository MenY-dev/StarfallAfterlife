using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class QuestListener
    {
        public DiscoveryQuest Info{ get; set; }

        public ServerCharacter Character { get; set; }

        public DiscoveryClient Client => Character?.DiscoveryClient;

        public SfaRealm Realm => Client?.Server?.Realm;

        public UserFleet Fleet { get; set; }

        public List<QuestConditionListener> Conditions { get; } = new();

        public int Id => Info?.Id ?? -1;

        public QuestState State { get; set; } = QuestState.InProgress;

        public static QuestListener Create(int questId, ServerCharacter character, QuestProgress progress = null)
        {
            if (character is null)
                return null;

            if (character.DiscoveryClient.Server.Realm is SfaRealm realm &&
                realm.QuestsDatabase?.GetQuest(questId) is DiscoveryQuest quest &&
                JsonHelpers.ParseNodeUnbuffered(quest.Conditions.ToJsonString()) is JsonArray conditions)
            {
                var listener = new QuestListener
                {
                    Info = quest,
                    Character = character,
                };

                progress ??= new();

                foreach (var item in conditions)
                {
                    var condition = QuestConditionListener.Create(listener, item);

                    if (condition is null)
                        return null;

                    condition.LoadProgress(progress.GetProgress(condition.Identity) ?? new());
                    listener.Conditions.Add(condition);
                }

                return listener;
            }

            return null;
        }

        public void StartListening()
        {
            StopListening();

            foreach (var item in Conditions)
                item?.StartListening();
        }

        public void StopListening()
        {
            foreach (var item in Conditions)
                item?.StopListening();
        }

        public void Update()
        {
            foreach (var item in Conditions)
                item?.Update();
        }

        public void OnProgressChanged(QuestConditionListener condition)
        {
            var progress = new QuestProgress();

            Character?.DiscoveryClient?.SendQuestDataUpdate();
            Character?.DiscoveryClient?.SendQuestCompleteData(this);

            foreach (var item in Conditions ?? new())
            {
                var conditionProgress = item?.SaveProgress();

                if (conditionProgress is not null)
                    progress.SetProgress(item.Identity, conditionProgress);
            }

            Character.UpdateQuestProgress(Id, progress);
        }

        public void DeliverQuestItems()
        {
            foreach (var item in Conditions)
                item.DeliverQuestItems();

            Character?.DiscoveryClient?.SendFleetCargo();
        }

        public DiscoveryQuestBinding[] GetBindings()
        {
            var bindings = new List<DiscoveryQuestBinding>();

            foreach (var item in Conditions.SelectMany(c => c.CreateBindings() ?? new()))
                if (item is not null)
                    bindings.Add(item);

            if (bindings.Any(b => b.CanBeAccepted) == false)
                bindings.Add(new()
                {
                    ObjectId = Info.ObjectId,
                    ObjectType = (DiscoveryObjectType)Info.ObjectType,
                    SystemId = Info.ObjectSystem,
                    CanBeAccepted = true,
                });

            //if (bindings.Any(b => b.CanBeFinished) == false)
            //    bindings.Add(new()
            //    {
            //        ObjectId = Quest.ObjectId,
            //        ObjectType = (DiscoveryObjectType)Quest.ObjectType,
            //        SystemId = Quest.ObjectSystem,
            //        CanBeFinished = true,
            //    });

            return bindings.ToArray();
        }

        public QuestLocationInfo[] GetLocations()
        {
            var locations = new List<QuestLocationInfo>();

            foreach (var item in Conditions)
                if (item?.GetLocationInfo() is QuestLocationInfo location)
                    locations.Add(location);

            return locations.ToArray();
        }

        public bool CheckCompleteon()
        {
            return Conditions?.All(c => c.IsCompleted) ?? true;
        }

        public void FinishQuest()
        {
            bool isCompleted = Conditions.All(c => c.IsCompleted);

            if (isCompleted == true)
            {
                StopListening();
                State = QuestState.Done;

                if (Character is ServerCharacter character)
                {
                    character.DiscoveryClient?.SendQuestCompleteData(this);
                    character.ActiveQuests?.Remove(this);
                }
            }
        }

        public void RaiseAction(string identity, string data)
        {
            foreach (var item in Conditions)
                if (item is not null && item.Identity == identity)
                    item.RaiseAction(data);
        }

        public void RaiseInitialActions()
        {
            foreach (var item in Conditions)
                item?.RaiseInitialActions();
        }

        public List<DiscoveryDropRule> CreateDropRules()
        {
            return Conditions?
                .Select(q => q.CreateDropRules())
                .Where(r => r is not null)
                .SelectMany(r => r)
                .Where(r => r is not null)
                .ToList() ?? new();
        }
    }
}

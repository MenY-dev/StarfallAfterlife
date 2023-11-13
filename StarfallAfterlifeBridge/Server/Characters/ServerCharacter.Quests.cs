using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Characters
{
    public partial class ServerCharacter
    {
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
    }
}

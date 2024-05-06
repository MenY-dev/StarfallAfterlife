using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Houses;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DoctrineQuestListener : QuestListener
    {
        public int DoctrineId { get; set; }

        public static DoctrineQuestListener Create(ServerCharacter character, int doctrineId)
        {
            DoctrineQuestListener result = null;
            var realmInfo = character?.DiscoveryClient?.Server?.RealmInfo;
            var dtb = realmInfo?.Realm?.Database ?? SfaDatabase.Instance;

            realmInfo?.Use(_ =>
            {
                if (character?.GetHouse() is SfHouse house &&
                    house.GetMember(character) is HouseMember member &&
                    house.GetDoctrine(doctrineId) is HouseDoctrine doctrine)
                {
                    var ident = doctrine.Info.GetFullQuestIdent(house);

                    if (ident is null)
                        return;

                    var questLogic = doctrine.Info.GetQuestLogic(house, dtb);

                    if (questLogic?.Conditions is List<QuestConditionInfo> conditions)
                    {
                        var listener = new DoctrineQuestListener
                        {
                            DoctrineId = doctrine.Info.Id,
                            Character = character,
                            EndTime = doctrine.EndTime,
                            Info = new DiscoveryQuest
                            {
                                LogicId = questLogic.Id,
                                LogicName = questLogic.UniqueLogicIdentifier,
                                Type = QuestType.HouseDoctrine,
                                ObjectFaction = house.Faction,
                                ObjectType = (GalaxyMapObjectType)(byte)DiscoveryObjectType.HouseActionHolder,
                                ObjectId = house.Id,
                                Conditions = questLogic.Conditions
                                    .Select(c => JsonHelpers.ParseNodeUnbuffered(c))
                                    .ToJsonArray(),
                            },
                        };

                        foreach (var item in listener.Info.Conditions)
                        {
                            var condition = DoctrineQuestConditionListener.Create(listener, item);

                            if (condition is null)
                                continue;

                            condition.LoadProgress(new() { Progress = doctrine.Progress });
                            listener.Conditions.Add(condition);
                        }

                        result = listener;
                    }
                }
            });
            
            return result;
        }
    }
}

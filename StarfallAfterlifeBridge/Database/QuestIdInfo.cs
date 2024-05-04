using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct QuestIdInfo
    {
        // Default
        //     4    8   12   16   20   24   28    32
        // |0000-0000-0000-0000-0000-0000-0000-|0000
        // |Id                                 |Type

        // MainQuestLine
        //     4    8   12   16    20    24   28    32
        // |0000-0000-0000-0000-00|00-|0000-0000-|0000
        // |Id                    |Is |Faction   |Type
        // |                      |lvl|          |    
        // |                      |   |          |   

        // UniqueQuestLine
        //     4    8   12    16   20    24   28    32
        // |0000-0000-0000-|0000-0000-|0000-0000-|0000
        // |Id             |Target    |Faction   |Type
        // |               |faction   |          |    
        // |               |          |          |   

        public int LocalId;
        public QuestType Type;
        public Faction Faction;
        public Faction TargetFaction;
        public bool IsLevelingQuest;
        public bool IsDynamicQuest;

        public bool IsValidId => LocalId >= 0 && this switch
        {
            { Type: QuestType.MainQuestLine, LocalId: var id } => id <= (-1 >>> 14),
            { Type: QuestType.UniqueQuestLine, LocalId: var id } => id <= (-1 >>> 20),
            { LocalId: var id } => id <= (-1 >>> 4)
        };

        public int ToId()
        {
            return Type switch
            {
                QuestType.MainQuestLine =>
                    LocalId & (-1 >>> 14) |
                    ((IsLevelingQuest ? 1 : 0) << 18) |
                    ((int)Faction << 20) |
                    ((int)Type << 28) |
                    ((IsDynamicQuest ? 1 : 0) << 31),

                QuestType.UniqueQuestLine =>
                    LocalId & (-1 >>> 20) |
                    ((int)TargetFaction << 12) |
                    ((int)Faction << 20) |
                    ((int)Type << 28) |
                    ((IsDynamicQuest ? 1 : 0) << 31),

                _ =>
                    LocalId & (-1 >>> 4) |
                    ((int)Type << 28) |
                    ((IsDynamicQuest ? 1 : 0) << 31),
            };
        }

        public static QuestIdInfo Create(int id)
        {
            var quest = new QuestIdInfo()
            {
                Type = (QuestType)(id >>> 28),
                IsDynamicQuest = (byte)(id >>> 31) > 0
            };

            switch (quest.Type)
            {
                case QuestType.MainQuestLine:
                    quest.LocalId = id & (-1 >>> 14);
                    quest.Faction = (Faction)((id & (-1 >>> 4)) >>> 20);
                    quest.IsLevelingQuest = ((id & (-1 >>> 12)) >>> 18) == 1;
                    break;
                case QuestType.UniqueQuestLine:
                    quest.LocalId = id & (-1 >>> 20);
                    quest.Faction = (Faction)((id & (-1 >>> 4)) >>> 20);
                    quest.TargetFaction = (Faction)((id & (-1 >>> 12)) >>> 12);
                    break;
                default:
                    quest.LocalId = id & (-1 >>> 4);
                    break;
            }

            return quest;
        }

        public static bool IsDynamicQuestId(int id) => (byte)(id >>> 31) > 0;
    }
}

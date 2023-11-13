using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Quests.Conditions
{
    internal class ExploreRelictShipConditionListener : QuestConditionListener, ICharacterInstanceListener
    {
        public int Lvl { get; protected set; }

        public ExploreRelictShipConditionListener(QuestListener quest, JsonNode info) : base(quest, info)
        {
        }

        protected override void LoadConditionInfo(JsonNode doc)
        {
            base.LoadConditionInfo(doc);
            Lvl = (int?)doc?["lvl"] ?? -1;
        }

        void ICharacterInstanceListener.OnInstanceInteractEvent(ServerCharacter character, int systemId, string eventType, string eventData)
        {
            Update();

            if ("explore_relic_ship".Equals(eventType, StringComparison.InvariantCultureIgnoreCase) &&
                JsonHelpers.ParseNodeUnbuffered(eventData ?? "") is JsonObject doc &&
                (string)doc["quest_identifier"] is string identifier &&
                CreateIdentifier().Equals(identifier, StringComparison.InvariantCultureIgnoreCase))
                Progress = Math.Min(ProgressRequire, Progress + 1);
        }

        protected string CreateIdentifier() => $"{Quest?.Id}-{Identity}";

        public override List<QuestTileParams> CreateInstanceTileParams(int systemId, DiscoveryObjectType objectType, int objectId)
        {
            if (objectType == DiscoveryObjectType.SecretObject &&
                Quest?.Client?.Map?.GetSystem(systemId) is GalaxyMapStarSystem system &&
                system.Level == Lvl)
            {
                return new()
                {
                    new("BP_TG_RelictShip", new() { { "quest_ident", JsonValue.Create(CreateIdentifier()) } })
                };
            }

            return null;
        }
    }
}

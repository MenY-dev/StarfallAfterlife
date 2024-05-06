using StarfallAfterlife.Bridge.Houses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct HouseDoctrineInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonPropertyName("quest_ident")]
        public string QuestIdent { get; set; }

        [JsonPropertyName("quest_duration")]
        public double QuestDuration { get; set; }

        [JsonPropertyName("effect_duration")]
        public double EffectDuration { get; set; }

        [JsonPropertyName("effect")]
        public int Effect { get; set; }

        public string GetFullQuestIdent(SfHouse house) => QuestIdent switch
        {
            "doctrine_wartime" => "doctrine_wartime_" + house.Faction.GetName().ToLowerInvariant(),
            var v => v,
        };

        public QuestLogicInfo GetQuestLogic(SfHouse house, SfaDatabase database)
        {
            var ident = GetFullQuestIdent(house);

            if (ident is null)
                return null;

            return database.QuestsLogics.FirstOrDefault(
                l => ident.Equals(l.Value.UniqueLogicIdentifier, StringComparison.OrdinalIgnoreCase)).Value;
        }
    }
}

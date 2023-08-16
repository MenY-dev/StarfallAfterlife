using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Realms
{
    public class SfaRealmProgress : SfaObject
    {
        public List<CharacterProgress> Characters { get; set; } = new();

        public override void LoadFromJson(JsonNode doc)
        {
            (Characters ??= new()).Clear();

            if (doc?["chars"] is JsonArray chars)
            {
                foreach (var item in chars)
                {
                    var charProgress = new CharacterProgress();
                    charProgress.LoadFromJson(item);
                    Characters.Add(charProgress);
                }
            }
        }

        public override JsonNode ToJson()
        {
            var chars = new JsonArray();

            if (Characters != null)
            {
                foreach (var item in Characters)
                    if (item != null)
                        chars.Add(item.ToJson());
            }

            return new JsonObject
            {
                ["chars"] = chars
            };
        }
    }
}

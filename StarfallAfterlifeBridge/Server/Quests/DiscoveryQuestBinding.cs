using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Bridge.Server.Quests
{
    public class DiscoveryQuestBinding : ICloneable
    {
        [JsonPropertyName("starsystem")]
        public int SystemId { get; set; }

        [JsonPropertyName("object_id")]
        public int ObjectId { get; set; }

        [JsonPropertyName("object_type")]
        public DiscoveryObjectType ObjectType { get; set; }

        [JsonPropertyName("can_be_accepted")]
        public bool CanBeAccepted { get; set; }

        [JsonPropertyName("can_be_finished")]
        public bool CanBeFinished { get; set; }

        public static DiscoveryQuestBinding Create(StarSystemObject obj, bool canBeAccepted, bool canBeFinished) =>
            new DiscoveryQuestBinding
            {
                SystemId = obj?.System?.Id ?? -1,
                ObjectId = obj?.Id ?? -1,
                ObjectType = obj?.Type ?? DiscoveryObjectType.None,
                CanBeAccepted = canBeAccepted,
                CanBeFinished = canBeFinished
            };

        object ICloneable.Clone() => Clone();

        public DiscoveryQuestBinding Clone()
        {
            var c = this;
            return c;
        }
    }
}

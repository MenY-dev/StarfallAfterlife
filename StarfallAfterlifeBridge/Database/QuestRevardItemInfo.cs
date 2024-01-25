using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct QuestRevardItemInfo
    {
        [JsonInclude, JsonPropertyName("item")]
        public int Id;

        [JsonInclude, JsonPropertyName("count")]
        public int Count;
    }
}

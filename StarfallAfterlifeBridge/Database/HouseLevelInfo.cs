using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public struct HouseLevelInfo
    {
        [JsonPropertyName("level")]
        public int Level {  get; set; }

        [JsonPropertyName("xp")]
        public long Xp { get; set; }
    }
}

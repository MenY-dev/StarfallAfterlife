using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ResearchInfo
    {
        [JsonPropertyName("entity")]
        public int Entity { get; set; }

        [JsonPropertyName("xp")]
        public int Xp { get; set; }

        [JsonPropertyName("is_opened")]
        public int IsOpened { get; set; }
    }
}

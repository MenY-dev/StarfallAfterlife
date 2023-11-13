using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class TileInfo
    {
        [JsonPropertyName("tile_name")]
        public string Name { get; set; }

        [JsonPropertyName("remove"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Remove { get; set; } = 0;

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 0;

        [JsonPropertyName("tiles_num_min")]
        public int NumMin { get; set; } = 0;

        [JsonPropertyName("tiles_num_max")]
        public int NumMax { get; set; } = 0;

        [JsonPropertyName("params")]
        public JsonObject Params { get; set; }
    }
}

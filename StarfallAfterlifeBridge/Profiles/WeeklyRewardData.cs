using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class WeeklyRewardData
    {
        [JsonPropertyName("ship_project_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int ShipProjectId { get; set; }

        [JsonPropertyName("item_project_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int ItemProjectId { get; set; }

        [JsonPropertyName("equipment_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int EquipmentId { get; set; }

        [JsonPropertyName("decal_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int DecalId { get; set; }

        [JsonPropertyName("skin_color_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int SkinColorId { get; set; }

        [JsonPropertyName("skin_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int SkinId { get; set; }
    }
}

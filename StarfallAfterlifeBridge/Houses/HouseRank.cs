using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public class HouseRank
    {
        [JsonPropertyName("id")]
        public int Id {  get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("permissions")]
        public HouseRankPermission Permissions {  get; set; }

        public static HouseRank CreateFromDatabaseInfo(HouseRankInfo info)
        {
            return new HouseRank
            {
                Id = info.Id,
                Order = info.Order,
                Permissions = (HouseRankPermission)info.Permissions
            };
        }
    }
}

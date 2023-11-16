using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class WeeklyReward
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("weekly_quest_stage")]
        public int Stage { get; set; }

        [JsonPropertyName("is_premium")]
        public int IsPremium { get; set; }

        [JsonPropertyName("type")]
        public WeeklyRewardType Type { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("data")]
        [JsonConverter(typeof(ObjectToJsonStringConverter<WeeklyRewardData>))]
        public WeeklyRewardData Data { get; set; }

        public static WeeklyReward CreateForShipProject(int id, int stage, int isPremium, int count, int shipId) => new()
        {
            Id = id,
            Stage = stage,
            IsPremium = isPremium,
            Count = count,
            Type = WeeklyRewardType.ShipProject,
            Data = new WeeklyRewardData() { ShipProjectId = shipId }
        };

        public static WeeklyReward CreateForEquipment(int id, int stage, int isPremium, int count, int equipmentId) => new()
        {
            Id = id,
            Stage = stage,
            IsPremium = isPremium,
            Count = count,
            Type = WeeklyRewardType.UniqueEquipment,
            Data = new WeeklyRewardData() { EquipmentId = equipmentId }
        };
    }
}

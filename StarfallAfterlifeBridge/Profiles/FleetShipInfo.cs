using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class FleetShipInfo : ICloneable
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("data")]
        public ShipConstructionInfo Data { get; set; } = new();

        [JsonPropertyName("position")]
        public int Position { get; set; } = 0;

        [JsonPropertyName("kills")]
        public int Kills { get; set; } = 0;

        [JsonPropertyName("death")]
        public int Death { get; set; } = 0;

        [JsonPropertyName("played")]
        public int Played { get; set; } = 0;

        [JsonPropertyName("woncount")]
        public int WonCount { get; set; } = 0;

        [JsonPropertyName("lostcount")]
        public int LostCount { get; set; } = 0;

        [JsonPropertyName("xp")]
        public int Xp { get; set; } = 0;

        [JsonPropertyName("level")]
        public int Level { get; set; } = 0;

        [JsonPropertyName("damagedone")]
        public int DamageDone { get; set; } = 0;

        [JsonPropertyName("damagetaken")]
        public int DamageTaken { get; set; } = 0;

        [JsonPropertyName("timetoconstruct")]
        public int TimeToConstruct { get; set; } = 0;

        [JsonPropertyName("timetorepair")]
        public int TimeToRepair { get; set; } = 0;

        [JsonPropertyName("is_favorite")]
        public int IsFavorite { get; set; } = 0;

        object ICloneable.Clone() => Clone();

        public FleetShipInfo Clone()
        {
            var clone = MemberwiseClone() as FleetShipInfo;
            clone.Data = Data?.Clone();
            return clone;
        }
    }
}
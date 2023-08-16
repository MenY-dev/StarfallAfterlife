using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Database
{
    public class DiscoveryMobInfo : ICloneable
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("InternalName")]
        public string InternalName { get; set; }

        [JsonPropertyName("Faction")]
        public Faction Faction { get; set; } = Faction.None;

        [JsonPropertyName("Level")]
        public int Level { get; set; } = -1;

        [JsonPropertyName("VisionRadius")]
        public int VisionRadius { get; set; } = -1;

        [JsonPropertyName("Speed")]
        public float Speed { get; set; } = -1;

        [JsonPropertyName("BehaviorTreeName")]
        public string BehaviorTreeName { get; set; }

        [JsonPropertyName("MainShipIndex")]
        public int MainShipIndex { get; set; } = -1;

        [JsonPropertyName("Tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("Ships")]
        public List<DiscoveryMobShipData> Ships { get; set; }

        public static readonly string[] ServiceFleetNames = new[]
        {
            "pve_service_fleet",
            "exam_fleet",
            "dpr_disc_fleet",
            "ecl_disc_fleet",
            "vng_disc_fleet",
            "screechers_fleets",
            "nebulords_fleet",
            "pyramid_fleets",
            "miners_fleets",
            "traders_fleet",
        };

        public int GetMainShipHull()
        {
            if (MainShipIndex > -1 && MainShipIndex < Ships.Count &&
                Ships[MainShipIndex]?.Data is ShipConstructionInfo data)
                return data.Hull;

            return Ships?.FirstOrDefault()?.Data?.Hull ?? 0;
        }

        object ICloneable.Clone() => Clone();

        public DiscoveryMobInfo Clone()
        {
            var clone = MemberwiseClone() as DiscoveryMobInfo;
            clone.Tags = Tags?.ToList();
            clone.Ships = Ships?.Select(i => i?.Clone())?.ToList();
            return clone;
        }


        public bool IsBoss() => Tags?.Contains("Mob.Role.Boss", StringComparer.InvariantCultureIgnoreCase) == true;

        public bool IsElite() => Tags?.Contains("Mob.Role.Elite", StringComparer.InvariantCultureIgnoreCase) == true;

        public bool IsServiceFleet() => ServiceFleetNames?.Contains(InternalName) == true;

        public IReadOnlyCollection<int> GetDropItems()
        {
            var items = new HashSet<int>();

            foreach (var item in Ships?
                .SelectMany(s => s?.GetDropItems() ?? Enumerable.Empty<int>()))
                items.Add(item);

            return items;
        }
    }
}

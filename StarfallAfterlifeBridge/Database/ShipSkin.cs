using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class ShipSkin
    {
        public int Id { get; set; } = -1;

        public string Name { get; set; }

        public Faction Faction { get; set; } = Faction.None;

        public bool IsDefault { get; set; } = false;

        public bool IsFactionReward { get; set; } = false;

        public ShipSkin(JsonNode doc)
        {
            Id = (int?)doc["id"] ?? -1;
            Name = (string)doc["name"] ?? string.Empty;
            Faction = (Faction?)(byte?)doc["faction"] ?? Faction.None;
            IsDefault = (int?)doc["is_default"] == 1;
            IsFactionReward = (int?)doc["is_faction_reward"] == 1;
        }
    }
}

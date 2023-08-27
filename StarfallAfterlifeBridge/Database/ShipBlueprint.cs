using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class ShipBlueprint : Blueprint
    {
        public string HullName { get; set; }

        public float HullStructure { get; set; } = 0;

        public float HullArmor { get; set; } = 0;

        public float HullShieldPoints { get; set; } = 0;

        public float HullShieldRegen { get; set; } = 0;

        public float HullCapacity { get; set; } = 0;

        public float HullSpeed { get; set; } = 0;

        public float HullMass { get; set; } = 0;

        public float HullWarpPoints { get; set; } = 0;

        public ShipClass HullClass { get; set; } = 0;

        public int CargoHoldSize { get; set; } = 0;

        public int TimeToConstruct { get; set; } = 0;

        public int TimeToRepair { get; set; } = 0;

        public int EntityLimit { get; set; } = -1;

        public List<int> Levels { get; set; } = new();

        public List<HardpointInfo> Hardpoints { get; set; } = new();

        public ShipBlueprint(JsonNode doc, JsonNode info) : base(doc, info)
        {
            if (info is null)
                return;

            ItemType = InventoryItemType.ShipProject;
            HullName = (string)info["HullName"];
            HullStructure = (float?)info["HullStructure"] ?? 0;
            HullArmor = (float?)info["HullArmor"] ?? 0;
            HullShieldPoints = (float?)info["HullShieldPoints"] ?? 0;
            HullShieldRegen = (float?)info["HullShieldRegen"] ?? 0;
            HullCapacity = (float?)info["HullCapacity"] ?? 0;
            HullSpeed = (float?)info["HullMaxSpeed"] ?? 0;
            HullMass = (float?)info["HullMass"] ?? 0;
            HullWarpPoints = (float?)info["HullWarpPoints"] ?? 0;
            HullClass = (ShipClass?)(byte?)info["hull_class"] ?? ShipClass.Unknown;
            CargoHoldSize = (int?)info["CargoHoldSize"] ?? 0;
            TimeToConstruct = (int?)info["time_to_construct"] ?? 0;
            TimeToRepair = (int?)info["full_repair_time"] ?? 0;
            EntityLimit = (int?)info["EntityLimit"] ?? 0;

            Hardpoints.Clear();

            if (info["hardpoints"]?.AsArray() is JsonArray hardpoints)
                foreach (var item in hardpoints)
                    if (item is not null &&
                        (string)item["name"] is string name &&
                        (TechType?)(int?)item["type"] is TechType type &&
                        (int?)item["x"] is int x &&
                        (int?)item["y"] is int y &&
                        (int?)item["width"] is int width &&
                        (int?)item["height"] is int height)
                        Hardpoints.Add(new HardpointInfo
                        {
                             Name = name,
                             Type = type,
                             X = x,
                             Y = y,
                             Width = width,
                             Height = height,
                        });

            Levels.Clear();
            Levels.AddRange(Enumerable.Repeat(0, 31));

            if (info["leveling"]?.AsArray() is JsonArray levels)
                foreach (var item in levels)
                    if (item is not null &&
                        (int?)item["level"] is int lvl && lvl > -1 && lvl < 31 &&
                        (int?)(float?)item["xp"] is int xp && xp > -1)
                        Levels[lvl] = xp;
        }
    }
}

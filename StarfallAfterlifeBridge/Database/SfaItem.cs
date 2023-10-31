using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class SfaItem
    {
        public int Id { get; set; } = -1;

        public string Name { get; set; }

        public int TechLvl { get; set; } = 0;

        public int IGC { get; set; } = 0;

        public int BGC { get; set; } = 0;

        public int Cargo { get; set; } = 0;

        public int MinLvl { get; set; } = 0;

        public int MaxLvl { get; set; } = 0;

        public double GalaxyValue { get; set; } = 0;

        public int ProductionPoints { get; set; } = 0;

        public int IGCToProduce { get; set; } = 0;

        public Faction Faction { get; set; } = Faction.None;

        public InventoryItemType ItemType { get; set; } = InventoryItemType.None;

        public int ProductionPointsOnDisassemble { get; set; } = 0;

        public int RequiredProjectToOpenXp { get; set; } = 0;

        public int ProjectToOpen { get; set; } = 0;

        public int ProjectToOpenXp { get; set; } = 0;

        public bool IsBoundToCharacter { get; set; } = false;

        public bool IsDefaultOpen { get; set; } = false;

        public bool IsImproved { get; set; } = false;

        public bool IsDefective { get; set; } = false;

        public int Width { get; set; } = 0;

        public int Height { get; set; } = 0;

        public int ProductItem { get; set; }

        public int ProductCount { get; set; }

        public List<MaterialInfo> Materials { get; } = new();

        public List<MaterialDropInfo> DisassembleMaterialsDrop { get; } = new();

        public List<BlueprintAbilityInfo> Abilities { get; } = new();

        public List<string> Tags { get; } = new();

        public int ProductionFrequency { get; set; } = 0;

        public int DisassemblyFrequency { get; set; } = 0;

        public SfaItem(JsonNode doc, JsonNode info)
        {
            Id = (int?)doc["id"] ?? -1;
            Name = (string)doc["name"] ?? string.Empty;
            TechLvl = (int?)doc["techlevel"] ?? 0;
            IGC = (int?)doc["igcprice"] ?? 0;
            BGC = (int?)doc["bgcprice"] ?? 0;
            Cargo = (int?)doc["cargo"] ?? 0;
            MinLvl = (int?)doc["min_galaxy_level"] ?? 0;
            MaxLvl = (int?)doc["max_galaxy_level"] ?? 0;
            GalaxyValue = (double?)doc["galaxy_value"] ?? 0;

            if (info is null)
                return;

            ProductionPoints = (int?)info["ProductionPoints"] ?? 0;
            IGCToProduce = (int?)info["IGCToProduce"] ?? 0;
            Faction = (Faction?)(byte?)info["Faction"] ?? Faction.None;
            ProductionPointsOnDisassemble = (int?)info["ProductionPointsOnDisassemble"] ?? 0;
            RequiredProjectToOpenXp = (int?)info["RequiredProjectToOpenXp"] ?? 0;
            ProjectToOpen = (int?)info["ProjectToOpen"] ?? 0;
            ProjectToOpenXp = (int?)info["ProjectToOpenXp"] ?? 0;
            IsBoundToCharacter = (int?)info["bCharacterBound"] == 1;
            IsDefaultOpen = (int?)info["ProjectIsDefaultOpen"] == 1;
            Width = (int?)info["ItemWidth"] ?? 0;
            Height = (int?)info["ItemHeight"] ?? 0;
            ProductItem = (int?)info["ProductItem"] ?? 0;
            ProductCount = (int?)info["ProductCount"] ?? 0;

            Materials.Clear();
            DisassembleMaterialsDrop.Clear();
            Abilities.Clear();
            Tags.Clear();

            if (info["materials"]?.AsArray() is JsonArray materials)
                foreach (var item in materials)
                    if (item is not null &&
                        (int?)item["Entity"] is int entity && entity > 0 &&
                        (int?)item["Count"] is int count && count > -1)
                        Materials.Add(new MaterialInfo { Id = entity, Count = count });

            if (info["DropItemsOnDisassemble"]?.AsArray() is JsonArray dropItems)
                foreach (var item in dropItems)
                    if (item is not null &&
                        (int?)item["Entity"] is int entity && entity > 0 &&
                        (int?)item["MinCount"] is int min && min > -1 &&
                        (int?)item["MaxCount"] is int max && max > -1)
                        DisassembleMaterialsDrop.Add(new MaterialDropInfo { Id = entity, Min = min, Max = max });

            if (info["Abilities"]?.AsArray() is JsonArray abilities)
                foreach (var item in abilities)
                    if (item is not null &&
                        (int?)item["Id"] is int id && id > 0 &&
                        (bool?)item["ConsumesItem"] is bool consumesItem)
                        Abilities.Add(new BlueprintAbilityInfo { Id = id, ConsumesItem = consumesItem });

            if (info["tags"]?.AsArray() is JsonArray tags)
                foreach (var item in tags)
                    if ((string)item is string tag)
                        Tags.Add(tag);

            IsImproved = Tags.Contains("Item.Quality.Improved", StringComparer.InvariantCultureIgnoreCase);
            IsDefective = Tags.Contains("Item.Quality.Broken", StringComparer.InvariantCultureIgnoreCase);
        }
    }
}

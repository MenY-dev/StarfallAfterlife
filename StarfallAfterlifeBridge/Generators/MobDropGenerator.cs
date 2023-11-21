using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class MobDropGenerator : GenerationTask
    {
        protected override bool Generate()
        {
            return true;
        }

        public DropTreeNode GenerateForMobShip(DiscoveryMobShipData ship, DiscoveryMobInfo mob = default)
        {
            var drop = new DropTreeNode()
            {
                Type = DropTreeNodeType.And,
                Chance = 1,
                Weight = 1,
                ItemMin = 1,
                ItemMax = 1,
                Childs = new(),
            };

            if (ship is null || ship.Data is null)
                return drop;

            mob ??= new() { Level = 100 };

            var database = SfaDatabase.Instance ?? new();
            var baseChance = ship.IsBoss() || ship.IsElite() ? 0.6f : 0.2f;
            var accessLvl = SfaDatabase.LevelToAccessLevel(mob.Level);
            var equipment = new HashSet<SfaItem>();
            var hardpoints = ship.Data?.HardpointList?
                .SelectMany(h => h.EquipmentList ?? new())
                .Select(h => h.Equipment);

            if ((mob.IsBoss() || mob.IsElite() == false) &&
                database.GetShip(ship.Data.Hull) is ShipBlueprint hull)
            {
                baseChance *= hull.HullClass switch
                {
                    ShipClass.Frigate => 0.5f,
                    ShipClass.Cruiser => 0.6f,
                    ShipClass.Battlecruiser => 0.7f,
                    ShipClass.Battleship => 0.8f,
                    ShipClass.Dreadnought => 1f,
                    ShipClass.Carrier => 1f,
                    _ => 1f,
                };
            }

            if (hardpoints is not null)
            {
                foreach (var h in hardpoints)
                {
                    var item = database.GetItem(h);

                    if (item  is null)
                        continue;

                    if ((item.IsImproved || item.IsDefective) &&
                        (item = database.GetItem(item.ProjectToOpen)) is null)
                        continue;

                    if (item.Faction.IsPirates() == false &&
                        item.Faction is not Faction.None or Faction.Other)
                        continue;

                    equipment.Add(item);
                }
            }

            foreach (var eq in equipment)
            {
                if (eq.MinLvl > accessLvl)
                    continue;

                var deltaLvl = Math.Max(1, accessLvl > eq.MaxLvl ? accessLvl - eq.MaxLvl : 1);
                var availableProjects = database.Equipments.Values
                    .Where(e => e.IsImproved == true && e.ProjectToOpen == eq.ProjectToOpen)
                    .SelectMany(e => database.DiscoveryItems.Values.Where(i => i.IsImproved == true && i.ProductItem == e.Id));

                if (eq.RequiredProjectToOpenXp < 0)
                    availableProjects = availableProjects.Concat(database.DiscoveryItems.Values
                        .Where(i => i.ProductItem == eq.Id));

                foreach (var project in availableProjects)
                {
                    if (project.MinLvl > accessLvl)
                        continue;

                    float projectChance;

                    if (ship.IsBoss() || ship.IsElite())
                    {
                        projectChance = baseChance * eq.TechLvl switch
                        {
                            < 2 => 0.4f,
                            2 => 0.6f,
                            3 => 0.8f,
                            _ => 1f,
                        };
                    }
                    else
                    {
                        projectChance = baseChance * eq.TechLvl switch
                        {
                            < 2 => 1f,
                            2 => 0.8f,
                            3 => 0.6f,
                            _ => 0.4f,
                        };
                    }

                    drop.Childs.Add(new DropTreeNode
                    {
                        Type = DropTreeNodeType.Item,
                        ItemId = project.Id,
                        ItemType = project.ItemType,
                        ItemMin = 1,
                        ItemMax = 2,
                        Weight = 1,
                        Chance = projectChance / deltaLvl,
                    });
                }

                drop.Childs.Add(new DropTreeNode
                {
                    Type = DropTreeNodeType.Item,
                    ItemId = eq.Id,
                    ItemType = eq.ItemType,
                    ItemMin = 1,
                    ItemMax = Math.Max(1, 4 - eq.TechLvl),
                    Weight = 1,
                    Chance = baseChance / deltaLvl * eq.TechLvl switch
                    {
                        < 2 => 0.3f,
                        2 => 0.25f,
                        3 => 0.2f,
                        _ => 0.15f,
                    },
                });
            }

            return drop;
        }
    }
}

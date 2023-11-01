using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Bridge.Server.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class ShopsGenerator : GenerationTask
    {
        public SfaRealm Realm { get; set; }

        public ShopsGenerator(SfaRealm realm)
        {
            if (realm is null)
                return;

            Realm = realm;
        }

        protected override bool Generate()
        {
            Realm.ShopsMap = Build();
            return true;
        }

        public virtual ShopsMap Build()
        {
            var map = new ShopsMap();
            GenerateShopsMap(map);
            return map;
        }


        protected virtual void GenerateShopsMap(ShopsMap map)
        {
            if (Realm?.GalaxyMap?.Systems is List<GalaxyMapStarSystem> systems)
            {
                foreach (var system in systems)
                {
                    foreach (var item in GetObjectsWithShops(system))
                    {
                        if (item is GalaxyMapPlanet planet &&
                            planet.Faction == Faction.None)
                            continue;

                        var shops = GenerateObjectShops(system, item);

                        if (shops is not null)
                            map.SetObjectShops(shops);
                    }
                }
            }
        }


        public virtual ObjectShops GenerateObjectShops(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed = 1)
        {
            static void TryAddShop(ObjectShops shops, ShopInfo shop)
            {
                if (shop is not null && shop.Items.Count > 0)
                    shops.Shops.Add(shop);
            }

            if (system is null || obj is null)
                return null;

            var shopsInfo = new ObjectShops
            {
                ObjectId = obj.Id,
                ObjectType = obj.ObjectType,
                Shops = new(),
            };

            seed += system.Id + obj.Id + (int)obj.ObjectType;
            var rnd = new Random(seed);

            var chance = obj.ObjectType switch
            {
                GalaxyMapObjectType.TradeStation => 2,
                GalaxyMapObjectType.ScienceStation => 3,
                _ => 5,
            };

            foreach (var generator in GetRequiredGeneratorsForObject(obj.ObjectType))
                TryAddShop(shopsInfo, generator(system, obj, seed));

            foreach (var generator in GetPossibleGeneratorsForObject(obj.ObjectType))
                if (rnd.Next() % chance == 0)
                    TryAddShop(shopsInfo, generator(system, obj, seed));

            if (shopsInfo.Shops.Count == 0)
                return null;

            return shopsInfo;
        }

        protected delegate ShopInfo ShopGeneratorFunc(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed);

        protected virtual IEnumerable<ShopGeneratorFunc> GetPossibleGeneratorsForObject(GalaxyMapObjectType objectType)
        {
            if (objectType is not (
                GalaxyMapObjectType.Blackmarket or
                GalaxyMapObjectType.FuelStation or
                GalaxyMapObjectType.MinerMothership or
                GalaxyMapObjectType.Planet or
                GalaxyMapObjectType.RepairStation or
                GalaxyMapObjectType.ScienceStation or
                GalaxyMapObjectType.TradeStation))
                yield break;

            if (objectType is not (GalaxyMapObjectType.TradeStation or
                                   GalaxyMapObjectType.ScienceStation))
            {
                yield return GenJunkieShop;
            }

            if (objectType is GalaxyMapObjectType.RepairStation)
            {
                yield return GenProjectsShop;
            }
            else
            {
                yield return GenEquipmentShop;
                yield return GenEquipmentLvl3Shop;
            }

            yield return GenCraftingShop;
            yield return GenGunsShop;
            yield return GenModulesShop;
            yield return GenAutonomousEquipmentShop;
        }

        protected virtual IEnumerable<ShopGeneratorFunc> GetRequiredGeneratorsForObject(GalaxyMapObjectType objectType)
        {
            switch (objectType)
            {
                case GalaxyMapObjectType.MinerMothership:
                    yield return GenOreShop;
                    yield break;
                case GalaxyMapObjectType.FuelStation:
                    yield return GenFuelShop;
                    yield break;
                case GalaxyMapObjectType.ScienceStation:
                    yield return GenImprovedEquipmentShop;
                    yield return GenImprovedEquipmentLvl3Shop;
                    yield break;
            }
        }

        protected virtual ShopInfo GenJunkieShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_junkie", StocName = "s_junkie" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.Equipments.Values)
                {
                    if (item.IsDefective == false ||
                        item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        rnd.Next() % 6 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenCraftingShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_crafting", StocName = "s_crafting" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.ItemType != InventoryItemType.DiscoveryItem ||
                        item.IsAvailableForTrading == false ||
                        item.Tags.Contains("Item.Role.Primitives", StringComparer.InvariantCultureIgnoreCase) == false ||
                        rnd.Next() % 3 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenProjectsShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_repair_station_bp", StocName = "s_repair_station_bp" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        item.Tags.Contains("Item.MiltipleItemBP", StringComparer.InvariantCultureIgnoreCase) == false ||
                        rnd.Next() % 6 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenGunsShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_weapon", StocName = "s_weapon" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.Equipments.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        TechType.Weapons.HasFlag(item.TechType) == false ||
                        item.IsImproved == true ||
                        item.IsDefective == true ||
                        rnd.Next() % 6 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenOreShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_resources", StocName = "s_resources" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.ItemType != InventoryItemType.DiscoveryItem ||
                        item.Tags.Contains("Item.Role.Ore", StringComparer.InvariantCultureIgnoreCase) == false)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenModulesShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_modules", StocName = "s_modules" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.Equipments.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        item.TechType != TechType.Engineering ||
                        item.IsImproved == true ||
                        item.IsDefective == true ||
                        rnd.Next() % 6 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenAutonomousEquipmentShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_consumable", StocName = "s_consumable" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Id == 724131681 || // Experimental fuel
                        item.Tags.Contains("Item.Role.Consumable", StringComparer.InvariantCultureIgnoreCase) == false ||
                        rnd.Next() % 3 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenImprovedEquipmentShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_dev_lab", StocName = "s_dev_lab" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        item.TechLvl > 2 ||
                        item.ItemType != InventoryItemType.ItemProject ||
                        item.IsImproved == false ||
                        rnd.Next() % 6 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenImprovedEquipmentLvl3Shop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_advanced_dev_lab", StocName = "s_advanced_dev_lab" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        item.TechLvl < 3 ||
                        item.ItemType != InventoryItemType.ItemProject ||
                        item.IsImproved == false ||
                        rnd.Next() % 3 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenEquipmentShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_engineering_department", StocName = "s_engineering_department" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        item.TechLvl > 2 ||
                        item.ItemType != InventoryItemType.ItemProject ||
                        item.IsImproved == true || 
                        rnd.Next() % 6 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenEquipmentLvl3Shop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_engineering_lab", StocName = "s_engineering_lab" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Faction is not (Faction.None or Faction.Other) ||
                        item.TechLvl < 3 ||
                        item.ItemType != InventoryItemType.ItemProject ||
                        item.IsImproved == true ||
                        rnd.Next() % 3 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected virtual ShopInfo GenFuelShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed);
            var shop = new ShopInfo { ShopName = "shop_fuel", StocName = "s_fuel" };

            if (GetCircleData(system) is SfaCircleData circle)
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.IsAvailableForTrading == false ||
                        item.Id != 724131681 || // Experimental fuel
                        rnd.Next() % 2 != 0)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }

        protected SfaCircleData GetCircleData(GalaxyMapStarSystem system) =>
            Realm.Database.CircleDatabase.TryGetValue(system?.Level ?? -1, out var circle) ? circle : null;

        public IEnumerable<IGalaxyMapObject> GetObjectsWithShops(GalaxyMapStarSystem system)
        {
            if (system is null)
                yield break;

            if (system.Planets is not null)
                foreach (var item in system.Planets)
                    yield return item;

            if (system.ScienceStations is not null)
                foreach (var item in system.ScienceStations)
                    yield return item;

            if (system.TradeStations is not null)
                foreach (var item in system.TradeStations)
                    yield return item;

            if (system.RepairStations is not null)
                foreach (var item in system.RepairStations)
                    yield return item;

            if (system.MinerMotherships is not null)
                foreach (var item in system.MinerMotherships)
                    yield return item;
        }
    }
}

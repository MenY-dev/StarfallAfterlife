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
    public class ShopsGenerator
    {
        public SfaRealm Realm { get; set; }

        public ShopsGenerator(SfaRealm realm)
        {
            if (realm is null)
                return;

            Realm = realm;
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


        protected virtual ObjectShops GenerateObjectShops(GalaxyMapStarSystem system, IGalaxyMapObject obj)
        {
            var shopsInfo = new ObjectShops
            {
                ObjectId = obj.Id,
                ObjectType = obj.ObjectType,
                Shops = new(),
            };

            var seed = system.Id + obj.Id + (int)obj.ObjectType;

            static void TryAddShop(ObjectShops shops, ShopInfo shop)
            {
                if (shop is not null && shop.Items.Count > 0)
                    shops.Shops.Add(shop);
            }

            TryAddShop(shopsInfo, GenerateOreShop(system, obj, seed));

            if (shopsInfo.Shops.Count == 0)
                return null;

            return shopsInfo;
        }


        protected virtual ShopInfo GenerateOreShop(GalaxyMapStarSystem system, IGalaxyMapObject obj, int seed)
        {
            var rnd = new Random(seed + 1);

            if ((rnd.Next() % 3) != 0)
                return null;

            var shop = new ShopInfo
            {
                ShopName = "shop_resources",
                StocName = "s_resources",
            };

            if (Realm.Database.CircleDatabase.TryGetValue(system.Level, out SfaCircleData circle))
            {
                foreach (var item in circle.DiscoveryItems.Values)
                {
                    if (item.ItemType != InventoryItemType.DiscoveryItem ||
                        item.Tags.Contains("Item.Role.Ore") == false)
                        continue;

                    shop.Items.Add(InventoryItem.Create(item, 999));
                }
            }

            return shop;
        }


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

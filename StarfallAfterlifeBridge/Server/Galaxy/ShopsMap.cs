using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization.Json;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class ShopsMap : SfaObject
    {
        public string Hash { get; set; }

        public Dictionary<ulong, ObjectShops> Shops { get; } = new();

        public ObjectShops GetObjectShops(int objectId, GalaxyMapObjectType objectType)
        {
            ulong hash = CreateBindingHash((byte)objectType, objectId);

            if (Shops.TryGetValue(hash, out ObjectShops shops))
                return shops;

            return null;
        }

        public ObjectShops GetObjectShops(int objectId, DiscoveryObjectType objectType)
        {
            ulong hash = CreateBindingHash((byte)objectType, objectId);

            if (Shops.TryGetValue(hash, out ObjectShops shops))
                return shops;

            return null;
        }

        public void SetObjectShops(ObjectShops shops)
        {
            if (shops is null)
                return;

            ulong hash = CreateBindingHash((byte)shops.ObjectType, shops.ObjectId);
            Shops[hash] = shops;
        }

        private static ulong CreateBindingHash(byte objectType, int objectId)
        {
            return (ulong)objectType << 48 | (uint)objectId;
        }

        public override void LoadFromJson(JsonNode doc)
        {
            static List<InventoryItem> JsonToInventory(JsonArray doc)
            {
                var inventory = new List<InventoryItem>();

                if (doc is not null && doc.Count > 0)
                {
                    foreach (var item in doc)
                    {
                        inventory.Add(new()
                        {
                            Id = (int?)item["id"] ?? -1,
                            Type = (InventoryItemType?)(byte?)item["type"] ?? InventoryItemType.None,
                            Count = (int?)item["count"] ?? -1,
                            IGCPrice = (int?)item["igc"] ?? -1,
                            BGCPrice = (int?)item["bgc"] ?? -1,
                        });
                    }
                }

                return inventory;
            }

            if (doc is null)
                return;

            Hash = (string)doc?["hash"];
            Shops?.Clear();

            if (doc["shops"]?.AsArray() is JsonArray shopsDoc)
            {
                foreach (var item in shopsDoc)
                {
                    if (item is null)
                        continue;

                    if ((int?)item["object_id"] is int id &&
                        (byte?)item["object_type"] is byte type)
                    {
                        var objectShops = new ObjectShops()
                        {
                            ObjectId = id,
                            ObjectType = (GalaxyMapObjectType)type,
                            Shops = new(),
                        };

                        if (item["shops"].AsArray() is JsonArray shops)
                        {
                            foreach (var shop in shops)
                            {
                                var shopInfo = new ShopInfo()
                                {
                                    ShopName = (string)shop["name"],
                                    StocName = (string)shop["stock"],
                                };

                                if (shop is not null &&
                                    shop["items"]?.AsArray() is JsonArray items)
                                {
                                    shopInfo.Items = JsonToInventory(items);
                                    objectShops.Shops.Add(shopInfo);
                                }
                            }
                        }

                        SetObjectShops(objectShops);
                    }
                }
            }
        }

        public override JsonNode ToJson()
        {
            static JsonArray InventoryToJson(List<InventoryItem> items)
            {
                var doc = new JsonArray();

                if (items is not null && items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        doc.Add(new JsonObject
                        {
                            ["id"] = item.Id,
                            ["type"] = (byte)item.Type,
                            ["count"] = item.Count,
                            ["igc"] = item.IGCPrice,
                            ["bgc"] = item.BGCPrice,
                        });
                    }
                }

                return doc;
            }

            var shopsDoc = new JsonArray();

            if (Shops is not null)
            {
                foreach (var item in Shops.Values)
                {
                    if (item is null)
                        continue;

                    var objectShops = new JsonArray();

                    foreach (var shop in item.Shops)
                    {
                        if (shop is null)
                            continue;

                        var inventory = InventoryToJson(shop.Items);

                        objectShops.Add(new JsonObject
                        {
                            ["name"] = shop.ShopName,
                            ["stock"] = shop.StocName,
                            ["items"] = inventory,
                        });
                    }

                    shopsDoc.Add(new JsonObject
                    {
                        ["object_id"] = item.ObjectId,
                        ["object_type"] = (byte)item.ObjectType,
                        ["shops"] = objectShops,
                    });
                }
            }

            return new JsonObject
            {
                ["hash"] = Hash,
                ["shops"] = shopsDoc,
            };
        }
    }
}

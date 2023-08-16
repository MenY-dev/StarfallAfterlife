using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Inventory
{
    public class CargoTransactionEndPoint
    {
        public string StockName { get; protected set; }

        protected ShopInfo _shop;
        protected ServerCharacter _character;
        protected EndPointType _type;

        protected enum EndPointType
        {
            Shop = 0,
            Inventory = 1,
            ShipCargo = 2,
            FleetCargo = 3,
        }

        public static CargoTransactionEndPoint CreateForCharacterStoc(ServerCharacter character, string stockName)
        {
            if (character is null)
                return null;

            return new CargoTransactionEndPoint
            {
                _type = stockName == "inventory" ? EndPointType.Inventory : EndPointType.ShipCargo,
                _character = character,
                StockName = stockName
            };
        }

        public static CargoTransactionEndPoint CreateForCharacterInventory(ServerCharacter character)
        {
            if (character is null)
                return null;

            return new CargoTransactionEndPoint
            {
                _type = EndPointType.Inventory,
                _character = character,
            };
        }

        public static CargoTransactionEndPoint CreateForCharacterFleet(ServerCharacter character, string targetStock = null)
        {
            if (character is null)
                return null;

            return new CargoTransactionEndPoint
            {
                _type = EndPointType.FleetCargo,
                _character = character,
                StockName = targetStock
            };
        }

        public static CargoTransactionEndPoint CreateForShop(ShopInfo shop)
        {
            if (shop is null)
                return null;

            return new CargoTransactionEndPoint
            {
                _type = EndPointType.Shop,
                _shop = shop,
            };
        }

        public int SendItemTo(CargoTransactionEndPoint target, int itemId, int count)
        {
            if (target is null || count < 1)
                return 0;

            ICharInventoryStorage storage = null;
            InventoryItem item = null;

            if (_type == EndPointType.Shop)
            {
                item = _shop?.Items?.FirstOrDefault(i => i.Id == itemId)?.Clone();

                if (item is null || item.Count < 1)
                    return 0;

                item.Count = count;
                return target.Receive(item);
            }

            if (_type == EndPointType.Inventory)
            {
                storage = _character?.Inventory;
            }
            else if (_type == EndPointType.ShipCargo || _type == EndPointType.FleetCargo)
            {
                if (_character is not null &&
                    _character.GetShipByStockName(StockName)?.Cargo is InventoryStorage cargo)
                {

                    storage = cargo;
                }
            }

            item = storage?[itemId]?.Clone();

            if (item is null || item.Count < 1)
                return 0;

            item.Count = Math.Min(item.Count, count);
            var totalCount = target.Receive(item);
            storage.Remove(itemId, totalCount);

            return totalCount;
        }

        public int Receive(SfaItem item, int count) =>
            item is null ? 0 : Receive(InventoryItem.Create(item, count));

        protected int Receive(InventoryItem item)
        {
            if (item is null)
                return 0;

            if (_type == EndPointType.Inventory)
            {
                if (_character?.Inventory is ICharInventoryStorage inventory &&
                    inventory.Add(item, item.Count) is not null)
                    return item.Count;
            }
            else if (_type == EndPointType.Shop)
            {
                return item.Count;
            }
            else if (_type == EndPointType.ShipCargo)
            {
                if (_character is not null &&
                    _character.GetShipByStockName(StockName) is ShipConstructionInfo ship &&
                    ship.Cargo is InventoryStorage cargo &&
                    _character?.DiscoveryClient?.Server?.Realm?.Database is SfaDatabase database &&
                    database.GetItem(item.Id) is SfaItem itemInfo)
                {
                    var usedCargo = database.CalculateUsedCargoSpace(cargo);
                    var freeSpace = ship.CargoHoldSize - usedCargo;

                    if (itemInfo.Cargo > 0)
                    {
                        var receivedCargo = Math.Min(item.Count, freeSpace / itemInfo.Cargo);
                        cargo.Add(item, receivedCargo);
                        return receivedCargo;
                    }
                    else
                    {
                        cargo.Add(item, item.Count);
                        return item.Count;
                    }
                }
            }
            else if (_type == EndPointType.FleetCargo)
            {
                if (_character is not null &&
                    _character.Ships is List<ShipConstructionInfo> ships &&
                    _character?.DiscoveryClient?.Server?.Realm?.Database is SfaDatabase database &&
                    database.GetItem(item.Id) is SfaItem itemInfo)
                {
                    if (_character.GetShipByStockName(StockName) is ShipConstructionInfo targetShip)
                    {
                        ships = new(ships);
                        ships.Remove(targetShip);
                        ships.Insert(0, targetShip);
                    }

                    var cargoCount = item.Count;
                    var totalReceivedCargo = 0;

                    foreach (var ship in ships)
                    {
                        if (ship.Cargo is InventoryStorage storage)
                        {
                            var usedCargo = database.CalculateUsedCargoSpace(storage);
                            var freeSpace = ship.CargoHoldSize - usedCargo;

                            if (freeSpace < itemInfo.Cargo)
                                continue;

                            if (itemInfo.Cargo > 0)
                            {
                                var receivedCargo = Math.Min(cargoCount, freeSpace / itemInfo.Cargo);
                                storage.Add(item, receivedCargo);
                                cargoCount -= receivedCargo;
                                totalReceivedCargo += receivedCargo;
                            }
                            else
                            {
                                storage.Add(itemInfo, cargoCount);
                                cargoCount = 0;
                                totalReceivedCargo = cargoCount;
                            }

                            if (cargoCount < 1)
                                break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    return totalReceivedCargo;
                }
            }

            return 0;
        }
    }
}

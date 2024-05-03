using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class Character
    {
        public void AddXp(int xp)
        {
            Xp = Xp.AddWithoutOverflow(xp);
            UpdateLevels();
        }

        public void UpdateLevels()
        {
            if (SfaDatabase.Instance.GetLevelInfoForCharXp(Xp) is LevelInfo info)
            {
                Level = Math.Max(Level, info.Level);
                AccessLevel = Math.Max(AccessLevel, info.AccessLevel);
                AbilityCells = Math.Max(AbilityCells, info.AbilityCells);
            }
        }

        public InventoryItem AddInventoryItem(SfaItem item, int count, string uniqueData = null)
        {
            if (Inventory is null)
                Inventory = new();

            Inventory.Add(item, count, uniqueData);
            return Inventory[item, uniqueData];
        }

        public InventoryItem AddInventoryItem(InventoryItem item, int count)
        {
            if (Inventory is null)
                Inventory = new();

            Inventory.Add(item, count);
            return Inventory[item];
        }

        public int DeleteInventoryItem(SfaItem item, int count = 1, string uniqueData = null)
        {
            if (Inventory is null)
                Inventory = new();

            return Inventory.Remove(item, count, uniqueData);
        }

        public InventoryItem GetInventoryItem(SfaItem item, string uniqueData = null)
        {
            if (Inventory is null)
                Inventory = new();

            return Inventory[item, uniqueData];
        }
        
        public InventoryItem[] GetInventoryItemVariants(SfaItem item)
        {
            if (Inventory is null)
                Inventory = new();

            return Inventory.GetAll(item);
        }

        public FleetShipInfo AddShip(int hull)
        {
            if (Ships is null)
                Ships = new();

            int id = CreateId(1, Ships, i => i.Id);

            if (id < 0)
                return null;

            FleetShipInfo ship = new()
            {
                Id = id,
                Data = new()
                {
                    Id = id,
                    Hull = hull,
                },
            };

            Ships.Add(ship);

            return ship;
        }

        public void DeleteShip(int id)
        {
            if (Ships is null)
                Ships = new();

            if (id < 0)
                return;

            for (int i = 0; i < Ships.Count; i++)
            {
                if (Ships[i].Id == id)
                {
                    Ships.RemoveAt(i);
                    break;
                }
            }
        }

        public void DeleteShip(FleetShipInfo ship)
        {
            if (ship is null)
                return;

            if (Ships is null)
                Ships = new();

            if (Ships.Contains(ship))
                Ships.Remove(ship);
        }

        public FleetShipInfo GetShip(int id)
        {
            if (Ships is null)
                Ships = new List<FleetShipInfo>();

            if (id < 0)
                return null;

            return Ships.FirstOrDefault(s => s.Id == id);
        }

        public CraftingInfo AddCraftingItem(int entity)
        {
            if (Crafting is null)
                Crafting = new List<CraftingInfo>();

            int id = CreateId(0, Crafting, i => i.CraftingId);

            if (id < 0)
                return null;

            CraftingInfo info = new CraftingInfo()
            {
                CraftingId = id,
                ProjectEntity = entity,
                QueuePosition = Crafting.Count > 0 ? Crafting.Max(c => c.QueuePosition) + 1 : 1,
                ProductionPointsSpent = 0
            };

            Crafting.Add(info);

            return info;
        }

        public void DeleteCraftingItem(int id)
        {
            if (Crafting is null)
                Crafting = new List<CraftingInfo>();

            if (id < 0)
                return;

            for (int i = 0; i < Crafting.Count; i++)
            {
                if (Crafting[i].CraftingId == id)
                {
                    Crafting.RemoveAt(i);
                    break;
                }
            }
        }

        public void DeleteCraftingItem(CraftingInfo item)
        {
            if (Crafting is null)
                Crafting = new List<CraftingInfo>();

            if (Crafting.Contains(item))
                Crafting.Remove(item);
        }

        public CraftingInfo GetCraftingItem(int id)
        {
            if (Crafting is null)
                Crafting = new List<CraftingInfo>();

            if (id < 0)
                return null;

            foreach (var item in Crafting)
                if (item.CraftingId == id)
                    return item;

            return null;
        }

        protected int CreateId<T>(int startID, IEnumerable<T> list, Func<T, int> walker)
        {
            if (startID < 0 || list is null || walker is null)
                return -1;

            List<T> items = new(list);
            int id = startID;

            for (int i = 0; i < items.Count; i++)
            {
                bool isEmpty = true;

                foreach (var item in items)
                {
                    if (walker.Invoke(item) == id)
                    {
                        isEmpty = false;
                        break;
                    }
                }

                if (isEmpty == true)
                    return id;

                id++;
            }

            return id;
        }
    }
}

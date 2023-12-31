﻿using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class ShipConstructionInfo : ICloneable, ICharInventoryStorage
    {

        [JsonPropertyName("elid")]
        public int Id { get; set; } = 0;

        [JsonPropertyName("plid")]
        public int FleetId { get; set; } = 0;

        [JsonPropertyName("hull")]
        public int Hull { get; set; } = 0;

        [JsonPropertyName("xp")]
        public int Xp { get; set; } = 0;

        [JsonPropertyName("armor")]
        public float ArmorDelta { get; set; } = 0;

        [JsonPropertyName("structure")]
        public float StructureDelta { get; set; } = 0;

        [JsonPropertyName("detachment")]
        public int Detachment { get; set; } = 0;

        [JsonPropertyName("slot")]
        public int Slot { get; set; } = 0;

        [JsonPropertyName("ship_skin")]
        public int ShipSkin { get; set; } = 0;

        [JsonPropertyName("skin_color_1")]
        public int SkinColor1 { get; set; } = 0;

        [JsonPropertyName("skin_color_2")]
        public int SkinColor2 { get; set; } = 0;

        [JsonPropertyName("skin_color_3")]
        public int SkinColor3 { get; set; } = 0;

        [JsonPropertyName("shipdecal")]
        public int ShipDecal { get; set; } = 0;

        [JsonPropertyName("hplist")]
        public List<ShipHardpoint> HardpointList { get; set; } = new();

        [JsonPropertyName("progression")]
        public List<ShipProgression> Progression { get; set; } = new();

        [JsonPropertyName("cargo_hold_size")]
        public int CargoHoldSize { get; set; } = 0;

        [JsonPropertyName("cargo_list")]
        public InventoryStorage Cargo { get; set; } = new();

        InventoryItem ICharInventoryStorage.this[int itemId, string uniqueData] => (Cargo ??= new())[itemId, uniqueData];

        object ICloneable.Clone() => Clone();

        public ShipConstructionInfo Clone()
        {
            var clone = MemberwiseClone() as ShipConstructionInfo;
            clone.HardpointList = HardpointList?.Select(i => i?.Clone())?.ToList();
            clone.Progression = Progression?.Select(i => i?.Clone())?.ToList();
            clone.Cargo = Cargo?.Clone();
            return clone;
        }

        public Dictionary<SfaItem, int> GetAllHardpointsEquipment()
        {
            var dtb = SfaDatabase.Instance;

            return HardpointList?
                .SelectMany(h => h?.EquipmentList ?? new())
                .Select(e => dtb.GetItem(e?.Equipment ?? 0))
                .Where(i => i is not null)
                .GroupBy(i => i)
                .ToDictionary(i => i.Key, i => i.Count()) ?? new();
        }

        public static int CalculateCost(ShipConstructionInfo from, ShipConstructionInfo to)
        {
            if (from is null || to is null)
                return 0;

            var totalCost = 0;

            int GetItemCost(int itemId) => SfaDatabase.Instance.GetItem(itemId) is SfaItem item ?
                (int)(Math.Pow(2, item.TechLvl + item.Width * item.Height) * 0.5) : 0;

            int CalcHardpointCoast(List<ShipHardpointEquipment> from, List<ShipHardpointEquipment> to)
            {
                var cost = 0;

                from ??= new();
                to ??= new();

                foreach (var fromItem in from)
                {
                    if (fromItem is null)
                        continue;

                    var toItem = to.FirstOrDefault(
                        i => i is not null && i.X == fromItem.X && i.Y == fromItem.Y);

                    var itemCost = GetItemCost(fromItem.Equipment);

                    if (toItem is null || toItem.Equipment != fromItem.Equipment)
                        cost += itemCost;
                }

                return cost;
            }

            foreach (var fromHp in from.HardpointList ?? new())
            {
                if (fromHp is null || fromHp.Hardpoint is null)
                    continue;

                var toHp = to.HardpointList?.FirstOrDefault(h =>
                    h is not null &&
                    h.Hardpoint?.Equals(fromHp.Hardpoint, StringComparison.InvariantCultureIgnoreCase) == true);

                if (toHp is null)
                    continue;

                totalCost += CalcHardpointCoast(fromHp.EquipmentList, toHp.EquipmentList);
                totalCost += CalcHardpointCoast(toHp.EquipmentList, fromHp.EquipmentList);
            }

            (int Cumulative, int Fixed) stepCost = (int)SfaDatabase.Instance.GetShip(to.Hull).HullClass switch
            {
                <= 1 => (20, 40),
                2 => (25, 75),
                3 => (30, 100),
                4 => (35, 125),
                5 => (40, 160),
                _ => (45, 200),
            };

            var startPointsSpend = from.Progression?.Sum(h => h?.Points ?? 0) ?? 0;
            var pointsSpend = to.Progression?.Sum(h => h?.Points ?? 0) ?? 0;
            int cumulativeCost = startPointsSpend * stepCost.Cumulative;

            for (int i = pointsSpend - startPointsSpend; i > 0; i--)
            {
                cumulativeCost += stepCost.Cumulative;
                totalCost += cumulativeCost + stepCost.Fixed;
            }

            return totalCost;
        }

        int ICharInventoryStorage.Add(InventoryItem item, int count)
        {

            if ((Cargo ??= new()) is InventoryStorage cargo &&
                SfaDatabase.Instance is SfaDatabase database &&
                database.GetItem(item.Id) is SfaItem itemInfo)
            {
                var freeSpace = CargoHoldSize - database.CalculateUsedCargoSpace(cargo);
                var receivedCargo = itemInfo.Cargo > 0 ? Math.Min(count, freeSpace / itemInfo.Cargo) : count;
                return cargo.Add(item, receivedCargo);
            }

            return 0;
        }

        int ICharInventoryStorage.Remove(int itemId, int count, string uniqueData)
        {
            return (Cargo ??= new()).Remove(itemId, count, uniqueData);
        }
    }
}

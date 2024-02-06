using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class RankedEquipmentLimitGenerator : GenerationTask
    {
        public List<InventoryItem> Limits { get; set; }

        private static readonly Dictionary<int, int> ShipConstants = new()
        {
            { 1955779012, 0 }, // Ursula
            { 352839448 , 0 }, // Prospero
            { 311661011 , 0 }, // BGProspero
            { 1107013852, 0 }, // Deprived Shield Neutralizer
            { 3944632   , 1 }, // Creed
            { 1042845690, 1 }, // Merit

            { 1012174624, 0 }, // Artaban
            { 392985104 , 0 }, // Vargas
            { 1978179795, 0 }, // BGVargas
            { 1005192487, 0 }, // Eclipse Shield Neutralizer
            { 1871570463, 1 }, // Acumen
            { 1343212635, 1 }, // Astu

            { 1855538177, 0 }, // Sampo
            { 1938143833, 0 }, // Kibisis
            { 5268544   , 0 }, // BGKibisis
            { 1985592629, 0 }, // Vanguard Shield Neutralizer
            { 789746730 , 1 }, // Valor
            { 1639619449, 1 }, // Duty
        };

        private static readonly Dictionary<int, int> EquipmentConstants = new()
        {
            { 820668078 , 0 }, // WeaponOverloadModule
            { 1336382816, 0 }, // ImprovedWeaponOverloadModule
            { 1770672044, 0 }, // DefectiveWeaponOverloadModule
            { 588002365 , 0 }, // LargeCombinedEngine
            { 260191900 , 0 }, // IndustrialMiningComplex
            { 1688154619, 0 }, // MiningComplex
        };

        protected override bool Generate()
        {
            Limits = Build();
            return true;
        }

        public List<InventoryItem> Build()
        {
            var limits = new List<InventoryItem>();
            var database = SfaDatabase.Instance;

            foreach (var ship in database.Ships.Values)
            {
                if (ship.Faction.IsMainFaction() == false ||
                    ship.MinLvl > 7 ||
                    ship.BGC > 0)
                    continue;

                var count = GetShipCount(ship);

                if (count < 1)
                    continue;

                limits.Add(InventoryItem.Create(ship, count));
            }

            foreach (var eq in database.Equipments.Values)
            {
                if (eq.Faction is not (Faction.Other or Faction.None) ||
                    eq.ProjectToOpenXp < 1 ||
                    eq.IsAvailableForTrading == false ||
                    eq.IsDefective == true ||
                    eq.IsImproved == true)
                    continue;

                var count = GetEquipmentCount(eq);

                if (count < 1)
                    continue;

                limits.Add(InventoryItem.Create(eq, count));
            }

            return limits;
        }

        public int GetShipCount(ShipBlueprint ship)
        {
            if (ship is not null &&
                ShipConstants.TryGetValue(ship.Id, out var count) == true)
                return count;

            return ship?.HullClass switch
            {
                ShipClass.Frigate => 3,
                ShipClass.Cruiser or
                ShipClass.Battlecruiser or
                ShipClass.Battleship => 2,
                ShipClass.Dreadnought => 1,
                _ => 0,
            };
        }

        public int GetEquipmentCount(EquipmentBlueprint eq)
        {
            if (eq is null)
                return 0;

            if (EquipmentConstants.TryGetValue(eq.Id, out var count) == true)
                return count;

            var techLvl = Math.Max(1, eq.TechLvl);
            var baseCount = eq.TechType switch
            {
                TechType.Engine => 200,
                TechType.Armor or
                TechType.Shield => 150,
                _ => 60,
            };

            return (int)Math.Round(
                (double)baseCount / (techLvl + eq.Width * eq.Height),
                MidpointRounding.ToEven) / 2 * 2;
        }
    }
}

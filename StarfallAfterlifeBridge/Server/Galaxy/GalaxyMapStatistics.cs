using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class GalaxyMapStatistics
    {
        public class GalaxyObjectStatistics
        {
            public int MinId { get; set; } = -1;
            public int MaxId { get; set; } = -1;
            public int Count { get; set; } = -1;

            public void Reset()
            {
                MinId = int.MinValue;
                MaxId = int.MinValue;
                Count = 0;
            }
        }

        public GalaxyObjectStatistics Systems { get; } = new();

        public GalaxyObjectStatistics Planets { get; } = new();

        public GalaxyObjectStatistics Portals { get; } = new();

        public GalaxyObjectStatistics Motherships { get; } = new();

        public GalaxyObjectStatistics PiratesStations { get; } = new();

        public GalaxyObjectStatistics RepairStations { get; } = new();

        public GalaxyObjectStatistics QuickTravalGates { get; } = new();

        public GalaxyObjectStatistics MinerMotherships { get; } = new();

        public GalaxyObjectStatistics ScienceStations { get; } = new();

        public GalaxyObjectStatistics Fuelstations { get; } = new();

        public GalaxyObjectStatistics Tradestations { get; } = new();

        public void Build(GalaxyMap galaxy)
        {
            var systems = galaxy?.Systems;

            Systems.Reset();
            Planets.Reset();
            Portals.Reset();
            Motherships.Reset();
            PiratesStations.Reset();
            RepairStations.Reset();
            QuickTravalGates.Reset();
            MinerMotherships.Reset();
            ScienceStations.Reset();
            Fuelstations.Reset();
            Tradestations.Reset();

            if (systems is null)
                return;

            UpdateObjectsGroupIds(systems, Systems);

            foreach (var system in systems)
            {
                if (system is null)
                    continue;

                UpdateObjectsGroupIds(system.Planets, Planets);
                UpdateObjectsGroupIds(system.Portals, Portals);
                UpdateObjectsGroupIds(system.Motherships, Motherships);
                UpdateObjectsGroupIds(system.PiratesStations, PiratesStations);
                UpdateObjectsGroupIds(system.RepairStations, RepairStations);
                UpdateObjectsGroupIds(system.QuickTravalGates, QuickTravalGates);
                UpdateObjectsGroupIds(system.MinerMotherships, MinerMotherships);
                UpdateObjectsGroupIds(system.ScienceStations, ScienceStations);
                UpdateObjectsGroupIds(system.FuelStations, Fuelstations);
                UpdateObjectsGroupIds(system.TradeStations, Tradestations);
            }
        }

        private void UpdateObjectsGroupIds<T>(List<T> group, GalaxyObjectStatistics stat) where T : IGalaxyMapObject
        {
            if (group is null || stat is null)
                return;

            foreach (var item in group)
            {
                if (item is null)
                    continue;

                stat.Count++;

                if (stat.MinId == int.MinValue || item.Id < stat.MinId)
                    stat.MinId = item.Id;

                if (stat.MaxId == int.MinValue || item.Id > stat.MaxId)
                    stat.MaxId = item.Id;
            }
        }
    }
}

using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MapEditor
{
    public class EditGalaxyMapViewModel : ViewModelBase
    {
        public bool IsSaved { get; protected set; } = false;

        public Faction[] FactionItems { get; } = Enum.GetValues<Faction>();

        public PlanetType[] PlanetTypeItems { get; } = Enum.GetValues<PlanetType>();

        public StarType[] StarTypeItems { get; } = Enum.GetValues<StarType>();

        protected virtual void Apply()
        {
            IsSaved = true;
        }

        protected int FindEmtyFactionGroup(GalaxyMap map)
        {
            if (map is null)
                return 0;

            int maxGroup = -1;

            foreach (var system in map.Systems ?? new())
            {
                if (system is null)
                    continue;

                if (system.FactionGroup > maxGroup)
                    maxGroup = system.FactionGroup;

                foreach (var item in system.PiratesOutposts ?? new())
                    if (item is not null && item.FactionGroup > maxGroup)
                        maxGroup = item.FactionGroup;

                foreach (var item in system.PiratesStations ?? new())
                    if (item is not null && item.FactionGroup > maxGroup)
                        maxGroup = item.FactionGroup;
            }

            return maxGroup + 1;
        }
    }
}

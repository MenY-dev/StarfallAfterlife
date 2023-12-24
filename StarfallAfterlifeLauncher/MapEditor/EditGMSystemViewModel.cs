using Avalonia.Controls;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MapEditor
{
    public class EditGMSystemViewModel : EditGalaxyMapViewModel
    {
        public GalaxyMap Map { get; protected set; }
        public GalaxyMapStarSystem System { get; protected set; }

        public string Name { get => _name; set => SetAndRaise(ref _name, value); }

        public int Level { get => _level; set => SetAndRaise(ref _level, value); }

        public Faction Faction { get => _faction; set => SetAndRaise(ref _faction, value); }

        public int FactionGroup { get => _factionGroup; set => SetAndRaise(ref _factionGroup, value); }

        private string _name;
        private int _level;
        private Faction _faction = Faction.None;
        private int _factionGroup = -1;

        public EditGMSystemViewModel(GalaxyMap map, GalaxyMapStarSystem system)
        {
            Map = map;
            System = system;

            if (system is null)
                return;

            Name = system.Name;
            Level = system.Level;
            Faction = system.Faction;
            FactionGroup = system.FactionGroup;
        }

        public void Save() => Save(null);

        public void Save(object sender)
        {
            if (sender is Window window)
                window.Close();

            if (System is not null)
                Apply();
        }

        protected override void Apply()
        {
            base.Apply();

            if (System is GalaxyMapStarSystem system)
            {
                system.Name = Name;
                system.Level = Level;
                system.Faction = Faction;
                system.FactionGroup = FactionGroup;
            }
        }

        public void UseNewFactionGroup()
        {
            FactionGroup = FindEmtyFactionGroup(Map);
        }
    }
}

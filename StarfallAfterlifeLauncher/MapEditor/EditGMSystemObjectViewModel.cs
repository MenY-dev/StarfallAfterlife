using Avalonia;
using Avalonia.Controls;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Server.Galaxy;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Launcher.MapEditor
{
    public class EditGMSystemObjectViewModel<TObject> : EditGalaxyMapViewModel where TObject : GalaxyMapStarSystemObject
    {
        public GalaxyMap Map { get; }

        public GalaxyMapStarSystem System { get; }

        public TObject SystemObject { get; protected set; }

        public virtual GalaxyMapObjectType ObjectType => SystemObject?.ObjectType ?? GalaxyMapObjectType.None;

        public int Id => SystemObject?.Id ?? -1;

        public int X { get => _x; set => SetAndRaise(ref _x, value); }

        public int Y { get => _y; set => SetAndRaise(ref _y, value); }

        private int _x = 0;

        private int _y = 0;

        public EditGMSystemObjectViewModel(GalaxyMap map, GalaxyMapStarSystem system, GalaxyMapStarSystemObject systemObject)
        {
            if (systemObject is TObject)
                Load(systemObject as TObject);
            Map = map;
            System = system;
        }

        public void Save() => Save(null);

        public void Save(object sender)
        {
            if (sender is Window window)
                window.Close();

            if (SystemObject is not null)
                Apply();
        }

        protected virtual void Load(TObject systemObject)
        {
            SystemObject = systemObject;
            X = systemObject.X;
            Y = systemObject.Y;
        }

        protected override void Apply()
        {
            base.Apply();
            SystemObject.X = X;
            SystemObject.Y = Y;
        }
    }

    public class EditGMSystemObjectViewModel : EditGMSystemObjectViewModel<GalaxyMapStarSystemObject>
    {
        public EditGMSystemObjectViewModel(GalaxyMap map, GalaxyMapStarSystem system, GalaxyMapStarSystemObject systemObject) : base(map, system, systemObject)
        {
        }
    }

    public class EditGMPiratesStationViwModel : EditGMSystemObjectViewModel<GalaxyMapPiratesStation>
    {
        public Faction Faction { get => _faction; set => SetAndRaise(ref _faction, value); }

        public int FactionGroup { get => _factionGroup; set => SetAndRaise(ref _factionGroup, value); }

        public int Level { get => _level; set => SetAndRaise(ref _level, value); }

        private Faction _faction;
        private int _factionGroup;
        private int _level;

        public EditGMPiratesStationViwModel(GalaxyMap map, GalaxyMapStarSystem system, GalaxyMapStarSystemObject systemObject) : base(map, system, systemObject)
        {
        }

        protected override void Load(GalaxyMapPiratesStation systemObject)
        {
            base.Load(systemObject);
            Faction = systemObject.Faction;
            FactionGroup = systemObject.FactionGroup;
            Level = systemObject.Level;
        }

        protected override void Apply()
        {
            base.Apply();
            SystemObject.Faction = Faction;
            SystemObject.FactionGroup = FactionGroup;
            SystemObject.Level = Level;
        }

        public void UseNewFactionGroup()
        {
            FactionGroup = FindEmtyFactionGroup(Map);
        }
    }

    public class EditGMPlanetViewModel : EditGMSystemObjectViewModel<GalaxyMapPlanet>
    {
        public string Name { get => _name; set => SetAndRaise(ref _name, value); }

        public PlanetType Type { get => _type; set => SetAndRaise(ref _type, value); }

        public int Size { get => _size; set => SetAndRaise(ref _size, value); }

        public float Temperature { get => _temperature; set => SetAndRaise(ref _temperature, value); }

        public float Atmosphere { get => _atmosphere; set => SetAndRaise(ref _atmosphere, value); }

        public float Gravitation { get => _gravitation; set => SetAndRaise(ref _gravitation, value); }

        public float NoubleGases { get => _noubleGases; set => SetAndRaise(ref _noubleGases, value); }

        public float RadiactiveMetals { get => _radiactiveMetals; set => SetAndRaise(ref _radiactiveMetals, value); }

        public float SuperConductors { get => _superConductors; set => SetAndRaise(ref _superConductors, value); }

        public Faction Faction { get => _faction; set => SetAndRaise(ref _faction, value); }

        private string _name;
        private PlanetType _type;
        private int _size;
        private float _temperature;
        private float _atmosphere;
        private float _gravitation;
        private float _noubleGases;
        private float _radiactiveMetals;
        private float _superConductors;
        private Faction _faction;

        public EditGMPlanetViewModel(GalaxyMap map, GalaxyMapStarSystem system, GalaxyMapStarSystemObject systemObject) : base(map, system, systemObject)
        {
        }

        protected override void Load(GalaxyMapPlanet systemObject)
        {
            base.Load(systemObject);
            Name = systemObject.Name;
            Type = systemObject.Type;
            Size = systemObject.Size;
            Temperature = systemObject.Temperature;
            Atmosphere = systemObject.Atmosphere;
            Gravitation = systemObject.Gravitation;
            NoubleGases = systemObject.NoubleGases;
            RadiactiveMetals = systemObject.RadiactiveMetals;
            SuperConductors = systemObject.SuperConductors;
            Faction = systemObject.Faction;
        }

        protected override void Apply()
        {
            base.Apply();
            SystemObject.Name = Name;
            SystemObject.Type = Type;
            SystemObject.Size = Size;
            SystemObject.Temperature = Temperature;
            SystemObject.Atmosphere = Atmosphere;
            SystemObject.Gravitation = Gravitation;
            SystemObject.NoubleGases = NoubleGases;
            SystemObject.RadiactiveMetals = RadiactiveMetals;
            SystemObject.SuperConductors = SuperConductors;
            SystemObject.Faction = Faction;
        }
    }
}

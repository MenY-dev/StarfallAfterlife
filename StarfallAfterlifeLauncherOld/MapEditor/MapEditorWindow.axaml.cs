using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MapEditor
{
    public partial class MapEditorWindow : Window
    {
        public static readonly StyledProperty<GalaxyMapStarSystem> SelectedSystemProperty =
            AvaloniaProperty.Register<MapEditorWindow, GalaxyMapStarSystem>(nameof(SelectedSystem), defaultBindingMode: BindingMode.OneWay);

        public static readonly StyledProperty<SystemHex?> SelectedHexProperty =
            AvaloniaProperty.Register<MapEditorWindow, SystemHex?>(nameof(SelectedHex), defaultBindingMode: BindingMode.OneWay);

        public static readonly StyledProperty<bool> SelectedHexIsNebulaProperty =
            AvaloniaProperty.Register<MapEditorWindow, bool>(nameof(SelectedHexIsNebula), defaultBindingMode: BindingMode.OneWay);

        public static readonly StyledProperty<bool> SelectedHexIsAsteroidsProperty =
            AvaloniaProperty.Register<MapEditorWindow, bool>(nameof(SelectedHexIsAsteroids), defaultBindingMode: BindingMode.OneWay);

        public ObservableCollection<IGalaxyMapObject> ObjectsInHex { get; } = new();

        public SystemHex? SelectedHex
        {
            get => GetValue(SelectedHexProperty);
            set => SetValue(SelectedHexProperty, value);
        }

        public bool SelectedHexIsNebula
        {
            get => GetValue(SelectedHexIsNebulaProperty);
            set => SetValue(SelectedHexIsNebulaProperty, value);
        }

        public bool SelectedHexIsAsteroids
        {
            get => GetValue(SelectedHexIsAsteroidsProperty);
            set => SetValue(SelectedHexIsAsteroidsProperty, value);
        }

        public GalaxyMapStarSystem SelectedSystem
        {
            get => GetValue(SelectedSystemProperty);
            set => SetValue(SelectedSystemProperty, value);
        }

        public GalaxyMap Map { get; protected set; }


        public MapEditorWindow()
        {
            DataContext = this;
            InitializeComponent();

            MapView.SystemChanged += OnSystemChanged;
            MapView.HexChanged += OnHexChanged;
            MapView = this.FindControl<MapView>("MapView");
        }

        public void LoadDefaultMap()
        {
            try
            {
                Map = JsonHelpers.DeserializeUnbuffered<GalaxyMap>(
                    File.ReadAllText(Path.Combine(".", "Database", "DefaultMap.json")));

                Map.Init();
                LoadMap(Map);
            }
            catch { }
        }

        public void OpenMap()
        {
            try
            {
                StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Galaxy Map",
                    FileTypeFilter = new FilePickerFileType[] { new("Galaxy Map") { Patterns = new[] { "*.json" } } },
                    AllowMultiple = false
                }).ContinueWith(t =>
                {
                    try
                    {
                        if (t.Result.Count > 0)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                using var stream = t.Result.First().OpenReadAsync().Result;
                                using var reader = new StreamReader(stream);
                                Map = JsonHelpers.DeserializeUnbuffered<GalaxyMap>(reader.ReadToEnd());
                                Map.Init();
                                LoadMap(Map);
                            });
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        public void SaveMap()
        {
            if (Map is null)
                return;

            try
            {
                StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Galaxy Map",
                    DefaultExtension = ".json",
                    FileTypeChoices = new FilePickerFileType[] { new("Galaxy Map") { Patterns = new[] { "*.json" } } },
                }).ContinueWith(t =>
                {
                    try
                    {
                        if (t.Result is not null)
                        {
                            var mapText = JsonHelpers.SerializeUnbuffered(Map);
                            using var stream = t.Result.OpenWriteAsync().Result;
                            using var writer = new StreamWriter(stream);
                            writer.Write(mapText);
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }

        protected void LoadMap(GalaxyMap map)
        {
            if (map is null)
                return;

            MapView.Map = Map;
            SelectedSystem = null;
            UpdateObjectsInHex(-1);
        }

        private void OnHexChanged(object sender, System.EventArgs e)
        {
            if (sender is MapView mapView)
            {
                SelectedSystem = Map?.GetSystem(mapView.SelectedSystem);
                var hexId = mapView.SelectedHex;

                if (SystemHexMap.ArrayIndexToHex(hexId, out var hex) == true)
                {
                    SelectedHex = hex;
                    SelectedHexIsAsteroids = new SystemHexMap(SelectedSystem?.AsteroidsMask)[hexId];
                    SelectedHexIsNebula = new SystemHexMap(SelectedSystem?.NebulaMask)[hexId];
                }
                else
                {
                    SelectedHex = null;
                    SelectedHexIsAsteroids = false;
                    SelectedHexIsNebula = false;
                }
                
                UpdateObjectsInHex(mapView.SelectedHex);
            }
        }

        private void OnSystemChanged(object sender, System.EventArgs e)
        {
            if (sender is MapView mapView)
            {
                SelectedSystem = Map?.GetSystem(mapView.SelectedSystem);
                UpdateObjectsInHex(-1);
            }
        }


        public void UpdateObjectsInHex()
        {
            UpdateObjectsInHex(-1);
            UpdateObjectsInHex(SystemHexMap.HexToArrayIndex(SelectedHex ?? SystemHex.Zero));
            MapView?.InvalidateVisual();
        }

        public void UpdateObjectsInHex(int hexId)
        {
            ObjectsInHex.Clear();

            if (hexId < 0 || hexId >= SystemHexMap.HexesCount)
                return;

            var hex = SystemHexMap.ArrayIndexToHex(hexId);
            var objs = SelectedSystem?.GetObjectsAt(hex.X, hex.Y).ToList();

            foreach (var item in objs)
                ObjectsInHex.Add(item);
        }

        protected object CreateEditViewModel(GalaxyMapStarSystemObject obj) => obj switch
        {
            var o when o is GalaxyMapPlanet => new EditGMPlanetViewModel(Map, SelectedSystem, obj),
            var o when o is GalaxyMapPiratesStation => new EditGMPiratesStationViwModel(Map, SelectedSystem, obj),
            _ => new EditGMSystemObjectViewModel(Map, SelectedSystem, obj),
        };

        public void EditObject(object obj)
        {
            var vm = CreateEditViewModel(obj as GalaxyMapStarSystemObject);
            ShowEditObjectWindow(vm);
            Trace.WriteLine($"Edit {obj}");
        }

        protected Task ShowEditObjectWindow(object vm)
        {
            if (vm is not null)
                return new EditSystemObjectWindow() { DataContext = vm }.ShowDialog(this)
                    .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                    {
                        UpdateObjectsInHex();
                    }));

            return Task.CompletedTask;
        }

        public bool CanEditObject(object obj)
        {
            return true;
        }

        public void ShowDeleteObject(object obj)
        {
            ConfirmDeleteObject(obj, () => DeleteObject(obj));
        }

        public bool CanShowDeleteObject(object obj)
        {
            return obj is
                GalaxyMapPlanet or
                GalaxyMapPiratesStation;
        }

        protected void DeleteObject(object obj)
        {
            if (obj is GalaxyMapPlanet planet)
                SelectedSystem?.Planets?.Remove(planet);
            else if (obj is GalaxyMapPiratesOutpost piratesOutpost)
                SelectedSystem?.PiratesOutposts?.Remove(piratesOutpost);
            else if (obj is GalaxyMapPiratesStation piratesStation)
                SelectedSystem?.PiratesStations?.Remove(piratesStation);

            UpdateObjectsInHex();
            Trace.WriteLine($"Delete {obj}");
        }

        public void ConfirmDeleteObject(object obj, Action isTrue)
        {
            new DeleteObjectMessageBox() { DataContext = obj }.ShowDialog<bool>(this).ContinueWith(t =>
            {
                if (t.Result == true)
                    Dispatcher.UIThread.Invoke(() => isTrue?.Invoke());
            });
        }

        public void EditSystem(object obj)
        {
            if (obj is GalaxyMapStarSystem system)
            {

                new EditGMSystemWindow() { DataContext = new EditGMSystemViewModel(Map, system) }.ShowDialog(this)
                    .ContinueWith(t => Dispatcher.UIThread.Invoke(() =>
                    {
                        SelectedSystem = null;
                        SelectedSystem = system;
                        UpdateObjectsInHex();
                    }));
            }
        }

        public void AddPiratesStation()
        {
            if (SelectedSystem is GalaxyMapStarSystem system &&
                SelectedHex is SystemHex hex)
            {
                var obj = new GalaxyMapPiratesStation()
                {
                    X = hex.X,
                    Y = hex.Y,
                    Faction = system.Faction,
                    FactionGroup = system.FactionGroup,
                    Level = system.Level,
                    Id = (Map?.Systems?.SelectMany(s => s.PiratesStations ?? new()).Max(s => s?.Id ?? -1) ?? -1) + 1,
                };

                var vm = new EditGMPiratesStationViwModel(Map, system, obj);

                ShowEditObjectWindow(vm).ContinueWith(t =>
                {
                    if (vm.IsSaved == true)
                    {
                        (system.PiratesStations ??= new()).Add(obj);
                        Dispatcher.UIThread.Invoke(UpdateObjectsInHex);
                    }
                });
            }
        }

        public void AddPiratesOutpost()
        {
            if (SelectedSystem is GalaxyMapStarSystem system &&
                SelectedHex is SystemHex hex)
            {
                var obj = new GalaxyMapPiratesOutpost()
                {
                    X = hex.X,
                    Y = hex.Y,
                    Faction = system.Faction,
                    FactionGroup = system.FactionGroup,
                    Level = system.Level,
                    Id = (Map?.Systems?.SelectMany(s => s.PiratesOutposts ?? new()).Max(s => s?.Id ?? -1) ?? -1) + 1,
                };

                var vm = new EditGMPiratesStationViwModel(Map, system, obj);

                ShowEditObjectWindow(vm).ContinueWith(t =>
                {
                    if (vm.IsSaved == true)
                    {
                        (system.PiratesOutposts ??= new()).Add(obj);
                        Dispatcher.UIThread.Invoke(UpdateObjectsInHex);
                    }
                });
            }
        }
    }
}

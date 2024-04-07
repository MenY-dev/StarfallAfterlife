using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.VisualBasic;
using StarfallAfterlife.Bridge.Codex;
using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.SfPackageLoader.FileSysten;
using StarfallAfterlife.Launcher.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CodexViewModel : ViewModelBase
    {

        public AppViewModel AppVM { get; }

        public SfaLauncher Launcher => AppVM?.Launcher;

        public SfCodex Codex { get; protected set; }

        public ObservableCollection<CodexEntryViewModel> Entries { get; } = new();

        public ObservableCollection<CodexEntryViewModel> Comparison { get; } = new();

        public bool ComparisonAvailable { get => _comparisonAvailable; set => SetAndRaise(ref _comparisonAvailable, value); }

        public bool ComparisonAddAvailable { get => _comparisonAddAvailable; set => SetAndRaise(ref _comparisonAddAvailable, value); }

        public bool ComparisonPanelVisible { get => _comparisonPanelVisible; set => SetAndRaise(ref _comparisonPanelVisible, value); }

        public CodexEntryViewModel TreeSelectedEntry
        {
            get => _treeSelectedEntry;
            set
            {
                SetAndRaise(ref _treeSelectedEntry, value);

                if (value is not CodexEntryGroupViewModel)
                    SelectedEntry = value;
            }
        }

        public CodexEntryViewModel SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                SetAndRaise(ref _selectedEntry, value);

                if (value is null)
                    SetAndRaise(ref _selectedItem, null, nameof(SelectedItem));
                else if (value is not CodexEntryGroupViewModel &&
                    value.Id is int itemId &&
                    Codex?.GetItem(itemId) is SfCodexItem item)
                    SetAndRaise(ref _selectedItem, new(this, item), nameof(SelectedItem));

                ComparisonAddAvailable = SelectedItem is not null;
            }
        }

        public CodexItemViewModel SelectedItem
        {
            get => _selectedItem;
        }

        public string Filter { get => _filter; set => SetAndRaise(ref _filter, value); }

        public bool LoadingStarted { get => _loadingStarted; set => SetAndRaise(ref _loadingStarted, value); }

        public bool ShowGameSelector { get => _showGameSelector; set => SetAndRaise(ref _showGameSelector, value); }

        private bool _comparisonAvailable;
        private bool _comparisonAddAvailable;
        private bool _comparisonPanelVisible;
        private string _filter;
        private CodexEntryViewModel _selectedEntry;
        private CodexEntryViewModel _treeSelectedEntry;
        private CodexItemViewModel _selectedItem;
        private bool _loadingStarted;
        private bool _showGameSelector;
        private string _lastLocalisation;

        public CodexViewModel()
        {
            if (Design.IsDesignMode == true)
                return;
        }

        public CodexViewModel(AppViewModel appVM)
        {
            if (Design.IsDesignMode == true)
                return;

            AppVM = appVM;
        }

        public void AddToComparison()
        {
            var entry = SelectedEntry;

            if (entry is null ||
                entry is CodexEntryGroupViewModel ||
                Comparison.Any(e => e.Id == entry.Id) == true)
                return;

            Comparison.Add(entry);
            ComparisonAvailable = true;
        }

        public void RemoveFromComparison(object item)
        {
            if (item is CodexEntryViewModel entry)
                Comparison.Remove(entry);

            if (Comparison.Count < 1)
            {
                ComparisonAvailable = false;
                ComparisonPanelVisible = false;
            }
        }

        public void ClearComparison()
        {
            Comparison.Clear();
            ComparisonAvailable = false;
            ComparisonPanelVisible = false;
        }

        public void Compare()
        {
            ComparisonPanelVisible = false;

            new CodexComparisonWindow()
            {
                DataContext = new CodexComparisonViewModel(Comparison.ToArray(), this),
            }.Show(App.MainWindow);
        }

        public void SetComparisonPanelState(object state)
        {
            if (state is bool isVisible)
                SetComparisonPanelState(isVisible);
        }

        public void SetComparisonPanelState(bool state) => ComparisonPanelVisible = state;

        protected override void OnPropertyChanged(object oldValue, object newValue, string name)
        {
            base.OnPropertyChanged(oldValue, newValue, name);

            if (name == nameof(Filter))
                UpdateFilter();
        }

        public void UpdateFilter()
        {
            var filter = string.IsNullOrWhiteSpace(Filter) ? string.Empty : Filter;

            int ProcessItems(ObservableCollection<CodexEntryViewModel> entries)
            {
                if (entries is null)
                    return 0;

                int visibleCount = 0;

                foreach (var entry in entries)
                {
                    if (entry is CodexEntryGroupViewModel group)
                    {
                        var count = ProcessItems(group.Items);
                        visibleCount += count;
                        group.Count = count;
                        group.IsVisible = count > 0;
                    }
                    else if (entry is CodexEntryViewModel item)
                    {
                        var visible = item.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true;
                        item.IsVisible = visible;

                        if (visible == true)
                            visibleCount++;
                    }
                }

                return visibleCount;
            }

            ProcessItems(Entries);
        }

        public SfCodex LoadCodexFromGame()
        {
            var launcher = Launcher;
            var appVM = AppVM;
            var gamePath = Launcher.GameDirectory;

            if (launcher is null ||
                appVM is null ||
                Directory.Exists(gamePath) == false)
                return null;

            var packsPath = Path.Combine(Launcher.GameDirectory, "Msk", "starfall_game", "Starfall", "Content", "Paks");
            var codex = SfCodex.LoadFromGame(packsPath) ?? new();

            if (launcher.WorkingDirectory is string workingDirectory)
            {
                var codexDir = Path.Combine(workingDirectory, "Codex");
                var codexFile = Path.Combine(codexDir, "codex.json");

                if (Directory.Exists(codexDir) == false)
                    Directory.CreateDirectory(codexDir);

                File.WriteAllText(codexFile, codex.ToJsonString());
            }

            return codex;
        }

        public SfCodex LoadCachedCodex()
        {
            SfCodex codex = null;

            try
            {
                var codexDir = Path.Combine(Launcher.WorkingDirectory, "Codex", "codex.json");
                codex = SfCodex.Load(codexDir);
            }
            catch { }

            return codex;
        }

        public void LoadEntries()
        {
            SelectedEntry = null;
            Filter = null;
            Entries.Clear();
            Comparison.Clear();

            var codex = Codex;

            if (codex is null)
                return;

            var ships = Codex.Ships ?? new();
            var equipment = Codex.Equipment ?? new();
            var discoveryItems = Codex.DiscoveryItems ?? new();

            Entries.Add(LoadShipsGroups(codex, ships));
            Entries.Add(LoadEquipmentGroup(codex, equipment));
            Entries.Add(LoadDiscoveryItemsGroup(codex, discoveryItems));

            UpdateFilter();
        }

        public string GetName(SfCodexItem item) =>
                item is null ? null :
                GetName(item.NameKey) ??
                item.Name ??
                item.Class;

        public string GetName(SfCodexTextKey key) =>
            GetName(key.Key, key.Namespace)?.Trim();

        public string GetName(string key, string tag = null) =>
                key is null ? null :
                Codex.GetText(key, App.CurrentLocalization, tag)?.Trim();

        public string GetDescription(SfCodexItem item) =>
                item is null ? null :
                Codex.GetText(item.DescriptionKey, App.CurrentLocalization)?.Trim();

        public string GetFactionName(Faction faction)
        {
            return Codex.GetText(faction switch
            {
                Faction.Pyramid => "PYRAMID",
                Faction.MineworkerUnion => "MineworkersUnion",
                _ => faction.ToString(),
            }, App.CurrentLocalization);
        }

        protected List<CodexEntryGroupViewModel> GroupBy<T>(
            IEnumerable<KeyValuePair<int, SfCodexItem>> items,
            Func<SfCodexItem, T> keySelector,
            Func<SfCodexItem, string> nameSelector,
            Func<T, string> groupNameSelector,
            Func<T, object> orderSelector)
        {
            var groups = new Dictionary<T, CodexEntryGroupViewModel>();

            if (keySelector is null)
                return new();

            var groupedItems = items
                .GroupBy(i => keySelector.Invoke(i.Value))
                .OrderBy(i => orderSelector.Invoke(i.Key));

            foreach (var itemsList in groupedItems)
            {
                var group = groups.GetValueOrDefault(itemsList.Key);

                if (group is null)
                {
                    groups[itemsList.Key] = group = new CodexEntryGroupViewModel()
                    {
                        Name = groupNameSelector.Invoke(itemsList.Key),
                        Items = new()
                    };
                }

                foreach (var item in itemsList)
                {
                    if (nameSelector.Invoke(item.Value) is string name)
                        group.Items.Add(new(){ Id = item.Key, Name = name });
                }

                group.Items.SortBy(i => i.Name);
            }

            return groups.Values.ToList();
        }

        protected CodexEntryGroupViewModel LoadDiscoveryItemsGroup(SfCodex codex, Dictionary<int, SfCodexItem> items)
        {
            var dtb = SfaDatabase.Instance;
            var group = new CodexEntryGroupViewModel()
            {
                Name = App.GetString("s_page_codex_group_items_lbl"),
                Items = new(GroupBy
                (
                    items: items.Where(
                        i => "EEquipmentQuality::EQDefective".Equals(
                            i.Value.Fields?.GetValueOrDefault("xlsEquipmentQuality")) == false),
                    keySelector: i =>
                    {
                        var key = i.BaseClass;

                        if (key == "AccessKeyItemProject")
                            key = "ItemProject";

                        return key;
                    },
                    nameSelector: i => GetName(i),
                    groupNameSelector: i => i switch
                    {
                        "ItemProject" => App.GetString("s_page_codex_group_items_projects_lbl"),
                        "DiscoveryCargo" => App.GetString("s_page_codex_group_items_cargo_lbl"),
                        _ => "Other"
                    },
                    orderSelector: i => i
                ) ?? new())
            };

            return group;
        }

        protected CodexEntryGroupViewModel LoadEquipmentGroup(SfCodex codex, Dictionary<int, SfCodexItem> items)
        {
            var dtb = SfaDatabase.Instance;
            var group = new CodexEntryGroupViewModel()
            {
                Name = App.GetString("s_page_codex_group_equipment_lbl"),
                Items = new(GroupBy
                (
                    items: items.Where(
                        i => "EEquipmentQuality::EQDefective".Equals(
                            i.Value.Fields?.GetValueOrDefault("xlsEquipmentQuality")) == false),
                    keySelector: i =>
                    {
                        var key = (dtb.GetItem(i?.Id ?? 0) as Blueprint)?.TechType ?? TechType.Unknown;
                        return key;
                    },
                    nameSelector: i => GetName(i),
                    groupNameSelector: i => GetName(i switch
                    {
                        TechType.Engineering => "Inventory",
                        _ => i.ToString(),
                    }),
                    orderSelector: i => i switch
                    {
                        TechType.Ballistic => 0,
                        TechType.Beam => 1,
                        TechType.Missile => 2,
                        TechType.Carrier => 3,
                        TechType.Armor => 4,
                        TechType.Shield => 5,
                        TechType.Engineering => 6,
                        TechType.Engine => 7,
                        _ => 8,
                    }
                ) ?? new())
            };

            return group;
        }

        protected CodexEntryGroupViewModel LoadShipsGroups(SfCodex codex, Dictionary<int, SfCodexItem> items)
        {
            var group = new CodexEntryGroupViewModel()
            {
                Name = App.GetString("s_page_codex_group_ships_lbl"),
                Items = new(GroupBy
                (
                    items: items,
                    keySelector: i =>
                    {
                        var key = Faction.Deprived;

                        if (i?.Fields?.GetValueOrDefault("ShipFaction") is Faction faction)
                            key = faction;

                        if (key == Faction.Other)
                            key = Faction.None;

                        return key;
                    },
                    nameSelector: i => GetName(i),
                    groupNameSelector: i => GetName(i switch
                    {
                        Faction.Pyramid => "PYRAMID",
                        Faction.MineworkerUnion => "MineworkersUnion",
                        Faction.None or
                        Faction.Other => "Unknown",
                        _ => i.ToString(),
                    }),
                    orderSelector: i => (int)i
                ) ?? new())
            };

            return group;
        }

        public void FastLoadCodex() => LoadCodex(false);

        public void FullLoadCodex() => LoadCodex(true);

        public void LoadCodex(bool ignoreCache)
        {
            Task.Factory.StartNew(() =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    LoadingStarted = true;
                });

                SfCodex codex = null;

                if (ignoreCache == false)
                {
                    codex = LoadCachedCodex();

                    if (codex is not null)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            Codex = codex;
                            LoadEntries();
                            LoadingStarted = false;
                            ShowGameSelector = false;
                        });

                        return;
                    }
                }

                if (Launcher?.TestGameDirectory() != true)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LoadEntries();
                        LoadingStarted = false;
                        ShowGameSelector = true;
                    });
                    return;
                }

                codex = LoadCodexFromGame();

                if (codex is not null)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Codex = codex;
                        LoadEntries();
                        LoadingStarted = false;
                        ShowGameSelector = false;
                    });

                    return;
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    LoadEntries();
                    LoadingStarted = false;
                    ShowGameSelector = true;
                });
            });
        }

        public void ShowGameDirSelector()
        {
            AppVM?.ShowGameDirSelector().ContinueWith(t =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    LoadingStarted = true;
                });

                if (Launcher?.TestGameDirectory() == true &&
                    LoadCodexFromGame() is SfCodex codex)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Codex = codex;
                        LoadEntries();
                        LoadingStarted = false;
                        ShowGameSelector = false;
                    });
                }
            });
        }

        public void OnPageShow()
        {
            if (_lastLocalisation != App.CurrentLocalization)
            {
                _lastLocalisation = App.CurrentLocalization;
                LoadEntries();
            }

            if (Codex is not null)
            {
                if (ShowGameSelector == true)
                    ShowGameSelector = false;

                if (LoadingStarted == true)
                    LoadingStarted = false;
            }
            else
            {
                FastLoadCodex();
            }
        }
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public partial class ShipSelector : Window
    {
        public static readonly StyledProperty<List<ShipBlueprint>> ShipsProperty =
            AvaloniaProperty.Register<ShipSelector, List<ShipBlueprint>>(nameof(Ships), defaultBindingMode: BindingMode.OneWay);

        public static readonly StyledProperty<ShipBlueprint> SelectionProperty =
            AvaloniaProperty.Register<ShipSelector, ShipBlueprint>(nameof(Selection));

        public static readonly StyledProperty<string> NameFilterProperty =
            AvaloniaProperty.Register<ShipSelector, string>(nameof(NameFilter));

        public static readonly StyledProperty<Faction> FactionFilterProperty =
            AvaloniaProperty.Register<ShipSelector, Faction>(nameof(FactionFilter), Faction.None);

        public static readonly StyledProperty<ShipClass> ShipClassFilterProperty =
            AvaloniaProperty.Register<ShipSelector, ShipClass>(nameof(ShipClassFilter), ShipClass.Unknown);

        public static readonly StyledProperty<TechType> TechTypeFilterProperty =
            AvaloniaProperty.Register<ShipSelector, TechType>(nameof(TechTypeFilter), TechType.Unknown);

        public List<ShipBlueprint> Ships { get => GetValue(ShipsProperty); set => SetValue(ShipsProperty, value); }

        public ShipBlueprint Selection { get => GetValue(SelectionProperty); set => SetValue(SelectionProperty, value); }

        public string NameFilter { get => GetValue(NameFilterProperty); set => SetValue(NameFilterProperty, value); }

        public Faction FactionFilter { get => GetValue(FactionFilterProperty); set => SetValue(FactionFilterProperty, value); }

        public ShipClass ShipClassFilter { get => GetValue(ShipClassFilterProperty); set => SetValue(ShipClassFilterProperty, value); }

        public TechType TechTypeFilter { get => GetValue(TechTypeFilterProperty); set => SetValue(TechTypeFilterProperty, value); }

        public ShipSelector()
        {
            DataContext = this;
            InitializeComponent();
            UpdateShips();
        }

        public ShipSelector(ShipSelector reference) : this()
        {
            if (reference is null)
                return;

            Selection = reference.Selection;
            NameFilter = reference.NameFilter;
            FactionFilter = reference.FactionFilter;
            ShipClassFilter = reference.ShipClassFilter;
        }

        public void UpdateShips()
        {
            var currentSelection = Selection;
            IEnumerable<ShipBlueprint> ships = SfaDatabase.Instance.Ships.Values;

            if (NameFilter?.Trim() is string nameFilter && string.IsNullOrWhiteSpace(NameFilter) == false)
                ships = ships.Where(s =>
                    s?.Name?.Contains(nameFilter, StringComparison.InvariantCultureIgnoreCase) == true ||
                    s?.HullName?.Contains(nameFilter, StringComparison.InvariantCultureIgnoreCase) == true);

            if (FactionFilter != Faction.None)
                ships = ships.Where(s => s.Faction == FactionFilter);

            if (ShipClassFilter != ShipClass.Unknown)
                ships = ships.Where(s => s.HullClass == ShipClassFilter);

            if (TechTypeFilter != TechType.Unknown)
                ships = ships.Where(s => s.Hardpoints?.Any(h => h.Type.HasFlag(TechTypeFilter)) == true);

            Ships = ships
                .OrderBy(s => (int)s.Faction)
                .ThenBy(s => (int)s.HullClass)
                .ThenBy(s => s.HullName)
                .ToList();

            Selection = currentSelection;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == NameFilterProperty ||
                change.Property == FactionFilterProperty ||
                change.Property == ShipClassFilterProperty ||
                change.Property == TechTypeFilterProperty)
            {
                UpdateShips();
            }
        }
    }
}

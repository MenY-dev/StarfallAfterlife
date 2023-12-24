using Avalonia.Data.Converters;
using Avalonia.Media;
using StarfallAfterlife.Bridge.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.Controls
{
    public static class SfaUtils
    {
        public static readonly Brush NoneFactionBrush = new SolidColorBrush(Color.FromArgb(255, 180, 150, 100));
        public static readonly Brush DeprivedFactionBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        public static readonly Brush EclipseFactionBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
        public static readonly Brush VanguardFactionBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        public static readonly Brush NeutralFactionBrush = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
        public static readonly Brush ScreechersFactionBrush = new SolidColorBrush(Color.FromArgb(255, 255, 80, 20));
        public static readonly Brush NebulordsFactionBrush = new SolidColorBrush(Color.FromArgb(255, 40, 80, 255));
        public static readonly Brush PyramidFactionBrush = new SolidColorBrush(Color.FromArgb(255, 100, 0, 200));
        public static readonly Brush FreeTradersFactionBrush = new SolidColorBrush(Color.FromArgb(255, 80, 200, 20));
        public static readonly Brush ScientistsFactionBrush = new SolidColorBrush(Color.FromArgb(255, 190, 210, 210));
        public static readonly Brush MineworkerUnionFactionBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        public static readonly Brush CriterionFactionBrush = new SolidColorBrush(Color.FromArgb(255, 100, 80, 170));

        public static Brush GetFactionBrush(Faction faction) => faction switch
        {
            Faction.Deprived => DeprivedFactionBrush,
            Faction.Eclipse => EclipseFactionBrush,
            Faction.Vanguard => VanguardFactionBrush,
            Faction.NeutralPlanets => NeutralFactionBrush,
            Faction.Screechers => ScreechersFactionBrush,
            Faction.Nebulords => NebulordsFactionBrush,
            Faction.Pyramid => PyramidFactionBrush,
            Faction.FreeTraders => FreeTradersFactionBrush,
            Faction.Scientists => ScientistsFactionBrush,
            Faction.MineworkerUnion => MineworkerUnionFactionBrush,
            Faction.Criterion => CriterionFactionBrush,
            _ => NoneFactionBrush
        };


        public static IEnumerable<Faction> FactionValues => Enum.GetValues<Faction>();

        public static IEnumerable<ShipClass> ShipClassValues => Enum.GetValues<ShipClass>();

        public static IEnumerable<TechType> TechTypeValues => Enum.GetValues<TechType>();

        public static readonly IValueConverter FactionToBrushConverter =
            new FuncValueConverter<Faction, Brush>(GetFactionBrush);



        public static readonly IValueConverter HardpointsToShipSpecConverter =
            new FuncValueConverter<IEnumerable<HardpointInfo>, string[]>(hs => hs
            .ToList()
            .Where(h => h.Type is TechType.Ballistic or TechType.Beam or TechType.Missile or TechType.Carrier)
            .GroupBy(h => h.Type)
            .Select(g => g.Key.ToString())
            .ToArray());
    }

}

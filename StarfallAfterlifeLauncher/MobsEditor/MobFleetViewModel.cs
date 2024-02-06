using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using HarfBuzzSharp;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Launcher.MapEditor;
using StarfallAfterlife.Launcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.MobsEditor
{
    public class MobFleetViewModel : ViewModelBase
    {
        public ObservableCollection<MobShipViewModel> Ships { get; } = new();

        public int Id
        {
            get => Info.Id;
            set
            {
                var oldValue = Info.Id;
                Info.Id = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public int Level
        {
            get => Info.Level;
            set
            {
                var oldValue = Info.Level;
                Info.Level = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public int Vision
        {
            get => Info.VisionRadius;
            set
            {
                var oldValue = Info.VisionRadius;
                Info.VisionRadius = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public float Speed
        {
            get => Info.Speed;
            set
            {
                var oldValue = Info.Speed;
                Info.Speed = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public string Name
        {
            get => Info.InternalName;
            set
            {
                var oldValue = Info.InternalName;
                Info.InternalName = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public string BehaviorTreeName
        {
            get => Info.BehaviorTreeName;
            set
            {
                var oldValue = Info.BehaviorTreeName;
                Info.BehaviorTreeName = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public Faction Faction
        {
            get => Info.Faction;
            set
            {
                var oldValue = Info.Faction;
                Info.Faction = value;
                RaisePropertyChanged(oldValue, value);
            }
        }

        public int MainShipIndex
        {
            get => Info.MainShipIndex;
            set
            {
                var oldValue = Info.MainShipIndex;
                Info.MainShipIndex = value;
                RaisePropertyChanged(oldValue, value);
                UpdateShips();
            }
        }

        public Faction[] FactionItems { get; } = Enum.GetValues<Faction>();

        public static string[] BTValues { get; } = new string[]
        {
            null,
            "/Game/gameplay/ai/fleets/BT_DefaultDiscoveryFleet.BT_DefaultDiscoveryFleet",
            "/Game/gameplay/ai/fleets/BT_Fleet_BoxPatrol.BT_Fleet_BoxPatrol",
            "/Game/gameplay/ai/fleets/BT_Fleet_Empty.BT_Fleet_Empty",
            "/Game/gameplay/ai/fleets/BT_Fleet_Miners.BT_Fleet_Miners",
            "/Game/gameplay/ai/fleets/BT_Fleet_StarHammerMarker.BT_Fleet_StarHammerMarker",
            "/Game/gameplay/ai/fleets/pyramid/BT_FleetPyramidCollectors.BT_FleetPyramidCollectors",
            "/Game/gameplay/ai/fleets/screechers/BT_ScreechersFleet.BT_ScreechersFleet",
        };

        public static readonly IValueConverter BTNames = new FuncValueConverter<string, string>(val => val switch
        {
            var v when v == BTValues[1] => "Default Discovery Fleet",
            var v when v == BTValues[2] => "BoxPatrol",
            var v when v == BTValues[3] => "Empty",
            var v when v == BTValues[4] => "Miners",
            var v when v == BTValues[5] => "Star Hammer Marker",
            var v when v == BTValues[6] => "Pyramid Collectors",
            var v when v == BTValues[7] => "Screechers Fleet",
            null => "None",
            _ => val,
        });

        public static IMultiValueConverter IsMainShipConverter => new FuncMultiValueConverter<object, bool>(context =>
        {
            if (context.FirstOrDefault(c => c is MobShipViewModel) is MobShipViewModel ship &&
                context.FirstOrDefault(c => c is MobFleetViewModel) is MobFleetViewModel fleet)
                return fleet.Info?.Ships?.IndexOf(ship.Ship) == fleet.MainShipIndex;

            return false;
        });

        public static IMultiValueConverter TagCheckedConverter => new FuncMultiValueConverter<object, bool>(context =>
        {
            if (context.FirstOrDefault(c => c is TagNode) is TagNode tag &&
                context.FirstOrDefault(c => c is MobFleetViewModel) is MobFleetViewModel fleet)
            {
                string fullTag = tag.GetFullPath();
                return fleet.Tags?.Contains(fullTag, StringComparer.InvariantCultureIgnoreCase) == true;
            }

            return false;
        });

        public DiscoveryMobInfo Info
        {
            get => info;
            set
            {
                info = value;
                UpdateShips();
            }
        }

        private DiscoveryMobInfo info;

        public ICollection<string> Tags => Info?.Tags;

        public ObservableCollection<MobTagViewModel> AllMobTags { get; protected set; }

        public MobFleetViewModel()
        {
            AllMobTags = new ObservableCollection<MobTagViewModel>(
                SfaDatabase.Instance.MobTags.ChildNodes.Select(n => MobTagViewModel.Create(n, this)));
        }

        public void SetTag(string tag, bool value)
        {
            if (Info is DiscoveryMobInfo info)
            {
                var oldTags = info.Tags;
                var newTags = new HashSet<string>(oldTags ?? new(), StringComparer.InvariantCultureIgnoreCase);

                if (value == true)
                    newTags.Add(tag);
                else
                    newTags.Remove(tag);

                info.Tags = newTags.ToList();
                RaisePropertyChanged(oldTags, info.Tags, nameof(Tags));
            }
        }

        public bool GetTag(string tag)
        {
            if (tag is null)
                return false;

            return Info?.Tags?.Contains(tag, StringComparer.InvariantCultureIgnoreCase) ?? false;
        }

        public void AddShip(int hull)
        {
            Info?.Ships?.Add(new()
            {
                Data = new() { Hull = hull },
                ServiceData = new()
            });

            UpdateShips();
        }

        public void UpdateShips()
        {
            Ships.Clear();

            foreach (var item in Info?.Ships ?? new())
            {
                if (item is null)
                    continue;

                Ships.Add(new() { Ship = item, Mob = this });
            }
        }

        public void DeleteShip(object ship)
        {
            var mainShip = GetMainShipData();
            Info?.Ships?.Remove((ship as MobShipViewModel)?.Ship);
            MainShipIndex = Math.Max(0, Info?.Ships?.IndexOf(mainShip) ?? 0);
            UpdateShips();
            Trace.WriteLine($"Delete: {(ship as MobShipViewModel)?.Ship?.Data?.Hull}");
        }

        public void CopyShip(object ship)
        {
            if (Info?.Ships is List<DiscoveryMobShipData> ships &&
                (ship as MobShipViewModel)?.Ship is DiscoveryMobShipData shipData)
            {
                var mainShip = GetMainShipData();
                int pos = Math.Max(0, ships.IndexOf(shipData));
                ships.Insert(Math.Min(pos + 1, ships.Count), shipData.Clone());
                MainShipIndex = Math.Max(0, ships.IndexOf(mainShip));
                UpdateShips();
                Trace.WriteLine($"Copy: {shipData.Data?.Hull}");
            }
        }

        public void MoveUpShip(object ship)
        {
            if (Info?.Ships is List<DiscoveryMobShipData> ships &&
                (ship as MobShipViewModel)?.Ship is DiscoveryMobShipData shipData)
            {
                int currentPos = Math.Max(0, ships.IndexOf(shipData));

                if (currentPos != -1)
                    MoveShip(shipData, currentPos - 1);
            }
        }

        public void MoveDownShip(object ship)
        {
            if (Info?.Ships is List<DiscoveryMobShipData> ships &&
                (ship as MobShipViewModel)?.Ship is DiscoveryMobShipData shipData)
            {
                int currentPos = Math.Max(0, ships.IndexOf(shipData));

                if (currentPos != -1)
                    MoveShip(shipData, currentPos + 1);
            }
        }

        protected void MoveShip(DiscoveryMobShipData ship, int position)
        {
            var mainShip = GetMainShipData();

            if (ship is not null &&
                Info?.Ships is List<DiscoveryMobShipData> ships &&
                position > -1 && position < ships.Count &&
                ships.Remove(ship) == true)
            {
                ships.Insert(position, ship);
                MainShipIndex = Math.Max(0, ships.IndexOf(mainShip));
                UpdateShips();
                Trace.WriteLine($"MoveShip: {ship.Data?.Hull}");
            }
        }

        public void SetMainShip(object ship)
        {
            if (Info?.Ships is List<DiscoveryMobShipData> ships &&
                (ship as MobShipViewModel)?.Ship is DiscoveryMobShipData shipData)
            {
                MainShipIndex = Math.Max(0, ships.IndexOf(shipData));
                Trace.WriteLine($"SetMainShip: {shipData.Data?.Hull}");
            }
        }

        protected DiscoveryMobShipData GetMainShipData() => Info?.Ships?.ElementAtOrDefault(MainShipIndex);
    }
}

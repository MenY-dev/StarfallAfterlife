using Microsoft.VisualBasic;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class CharacterInfoViewModel : ViewModelBase
    {
        public string Name => Info?.Name;

        public Faction Faction => (Faction)(Info?.Faction ?? (int)Faction.None);

        public int Level => Info?.Level ?? 0;

        public int AccessLevel => Info?.AccessLevel ?? 0;

        public int IGC => Info?.IGC ?? 0;

        public int BGC => Info?.BGC ?? 0;

        public ObservableCollection<FleetShipInfoViewModel> Ships { get; } = new();

        public Character Info
        {
            get => _info;
            set
            {
                _info = value;

                UpdateShips();
                Update();
            }
        }

        private Character _info;

        public CharacterInfoViewModel()
        {
        }

        public CharacterInfoViewModel(Character info)
        {
            Info = info;
        }

        public void Update()
        {
            UpdateShips();

            RaisePropertyChanged(Name, nameof(Info));
            RaisePropertyChanged(Faction, nameof(Faction));
            RaisePropertyChanged(Level, nameof(Level));
            RaisePropertyChanged(AccessLevel, nameof(AccessLevel));
            RaisePropertyChanged(IGC, nameof(IGC));
            RaisePropertyChanged(BGC, nameof(BGC));
            
            foreach (var item in Ships.ToArray())
                item?.Update();
        }

        public void UpdateShips()
        {
            if (Info?.Ships?.ToArray() is FleetShipInfo[] infoShips)
            {
                var shipsVM = Ships.ToArray();

                foreach (var item in shipsVM)
                {
                    if (infoShips.Contains(item?.Info) == false)
                        Ships.Remove(item);
                }

                shipsVM = Ships.ToArray();

                for (int i = 0; i < infoShips.Length; i++)
                {
                    var item = infoShips[i];

                    if (shipsVM.Any(s => s?.Info == item) == false)
                        Ships.Insert(i, new(item));
                }
            }
        }
    }
}

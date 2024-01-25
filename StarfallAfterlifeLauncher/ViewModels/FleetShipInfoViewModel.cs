using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Launcher.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class FleetShipInfoViewModel : ViewModelBase
    {
        public string Name { get; protected set; }

        public int Level => Info?.Level ?? 0;

        public int Xp => Info?.Xp ?? 0;

        public bool IsFavorite => Info?.IsFavorite is not null or 0;

        public ShipConstructionInfo Data => Info?.Data;

        public FleetShipInfo Info
        {
            get => _info;
            set
            {
                _info = value;
                Update();
            }
        }

        private FleetShipInfo _info;

        public FleetShipInfoViewModel()
        {
        }

        public FleetShipInfoViewModel(FleetShipInfo info)
        {
            Info = info;
        }

        public void Update()
        {
            Name = SfaDatabase.Instance.GetShipName(Data?.Hull ?? -1);

            RaisePropertyChanged(Name, nameof(Name));
            RaisePropertyChanged(Level, nameof(Info));
            RaisePropertyChanged(Xp, nameof(Xp));
            RaisePropertyChanged(IsFavorite, nameof(IsFavorite));
            RaisePropertyChanged(Data, nameof(Data));
        }
    }
}

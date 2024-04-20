using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class ObjectNameReportsViewModel
    {
        public int Id { get; set; }

        public bool IsPlanet { get; set; } = false;

        public bool IsSystem { get; set; } = false;

        public string Name { get; set; }

        public string Author { get; set; }

        public ObservableCollection<string> Reports { get; set; }

        public RealmObjectNameReport ReportInfo { get; set; }
    }
}

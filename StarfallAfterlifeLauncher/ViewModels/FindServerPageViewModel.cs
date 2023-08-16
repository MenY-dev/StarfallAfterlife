using StarfallAfterlife.Bridge.Launcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class FindServerPageViewModel : ViewModelBase
    {
        public AppViewModel AppVM { get; }

        public SfaLauncher Launcher => AppVM?.Launcher;

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                SetAndRaise(ref _serverAddress, value);
            }
        }

        private string _serverAddress;

        public FindServerPageViewModel(AppViewModel appVM)
        {
            AppVM = appVM;
        }

        public void ConnectToServer()
        {
            try
            {
                string address = IPEndPoint.Parse(ServerAddress).ToString();

                if (Launcher is SfaLauncher launcher)
                {
                    launcher.StartGame(launcher.CurrentProfile, address);
                }
            }
            catch { }
        }
    }
}

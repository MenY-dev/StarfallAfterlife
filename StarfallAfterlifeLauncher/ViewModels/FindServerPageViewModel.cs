using Avalonia.Threading;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Launcher.Controls;
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
                    var writePasswordDialog = new Func<string>(() =>
                    {
                        var completionSource = new TaskCompletionSource<string>();

                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            var dialog = new EnterPasswordDialog();

                            dialog.ShowDialog().ContinueWith(t => Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (dialog.IsDone == true &&
                                    string.IsNullOrWhiteSpace(dialog.Text) == false &&
                                    SfaServer.CreatePasswordHash(dialog.Text) is string hash)
                                    completionSource.SetResult(hash);
                                else
                                    completionSource.SetResult(null);
                            }));

                        });

                        return completionSource.Task.Result;
                    });

                    launcher.StartGame(launcher.CurrentProfile, address, writePasswordDialog);
                }
            }
            catch { }
        }
    }
}

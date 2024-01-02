using Avalonia.Threading;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Launcher.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class RemoteServerInfoViewModel : ViewModelBase
    {
        public string Address => Info?.Address;

        public string Id => Info?.Id;

        public string Name => Info?.Name;

        public string Description => Info?.Description;

        public Version Version => Info?.Version;

        public bool IsOnline => Info?.IsOnline ?? false;

        public bool IsBadVersion => Version is not null && SfaServer.IsVersionCompatible(Version) == false;

        public bool NeedPassword => Info?.NeedPassword ?? false;

        public FindServerPageViewModel Page { get; }

        public RemoteServerInfo Info
        {
            get => _info;
            set
            {
                SetAndRaise(ref _info, value);
                RaiseModelChanged();
            }
        }

        private RemoteServerInfo _info;

        public RemoteServerInfoViewModel(FindServerPageViewModel page, RemoteServerInfo info)
        {
            Page = page;
            Info = info;
        }

        public void RaiseModelChanged()
        {
            RaisePropertyChanged(Info?.Address, nameof(Address));
            RaisePropertyChanged(Info?.Id, nameof(Id));
            RaisePropertyChanged(Info?.Name, nameof(Name));
            RaisePropertyChanged(Info?.Description, nameof(Description));
            RaisePropertyChanged(Info?.Version, nameof(Version));
            RaisePropertyChanged(Info?.IsOnline ?? false, nameof(IsOnline));
            RaisePropertyChanged(Info?.NeedPassword ?? false, nameof(NeedPassword));
        }

        public Task<bool> Update() =>
            Update(default);

        public Task<bool> Update(CancellationToken ct)
        {
            return Info?.Update(5000, ct).ContinueWith(t =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Info = Info;
                    Page?.UpdateList();
                });

                if (ct.IsCancellationRequested)
                    return false;

                return t.Result;
            });
        }

        protected override void OnPropertyChanged(object oldValue, object newValue, string name)
        {
            base.OnPropertyChanged(oldValue, newValue, name);
        }
    }
}

using Avalonia.Threading;
using StarfallAfterlife.Bridge.Launcher;
using StarfallAfterlife.Bridge.Profiles;
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

        public DiscoverySession[] Sessions { get; protected set; }

        public bool IsActiveSession => Sessions?.Any(
            s => s?.RealmId is not null && s.RealmId == Id) ?? false;

        public string[] ActiveSessionChars { get; protected set; }

        public FindServerPageViewModel Page { get; }

        protected bool IsUpdateStarted => UpdateTasksCount > 0;

        protected int UpdateTasksCount = 0;

        public RemoteServerInfo Info
        {
            get => _info;
            set
            {
                SetAndRaise(ref _info, value);

                if (Page?.Launcher is SfaLauncher launcher)
                {
                    Sessions = launcher.CurrentProfile?.GetSessions(Id);
                    ActiveSessionChars = Sessions?
                        .Where(s => s is not null)
                        .Select(s => launcher.CurrentProfile?.GetCharacter(s?.CharacterId ?? -1)?.Name)
                        .Where(n => n is not null)
                        .ToArray();
                }
                else
                {
                    Sessions = null;
                    ActiveSessionChars = null;
                }

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
            RaisePropertyChanged(Sessions, nameof(Sessions));
            RaisePropertyChanged(IsActiveSession, nameof(IsActiveSession));
            RaisePropertyChanged(ActiveSessionChars, nameof(ActiveSessionChars));
        }

        public Task<bool> Update() =>
            Update(default);

        public Task<bool> Update(CancellationToken ct)
        {
            UpdateTasksCount++;
            RaisePropertyChanged(IsUpdateStarted, nameof(IsUpdateStarted));

            return Info?.Update(5000, ct).ContinueWith(t =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    Info = Info;
                    Page?.UpdateList();
                });

                UpdateTasksCount--;
                RaisePropertyChanged(IsUpdateStarted, nameof(IsUpdateStarted));

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

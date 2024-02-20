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
            RaisePropertyUpdate(Info?.Address, nameof(Address));
            RaisePropertyUpdate(Info?.Id, nameof(Id));
            RaisePropertyUpdate(Info?.Name, nameof(Name));
            RaisePropertyUpdate(Info?.Description, nameof(Description));
            RaisePropertyUpdate(Info?.Version, nameof(Version));
            RaisePropertyUpdate(Info?.IsOnline ?? false, nameof(IsOnline));
            RaisePropertyUpdate(Info?.NeedPassword ?? false, nameof(NeedPassword));
            RaisePropertyUpdate(Sessions, nameof(Sessions));
            RaisePropertyUpdate(IsActiveSession, nameof(IsActiveSession));
            RaisePropertyUpdate(ActiveSessionChars, nameof(ActiveSessionChars));
        }

        public Task<bool> Update() =>
            Update(default);

        public Task<bool> Update(CancellationToken ct)
        {
            if (Info is RemoteServerInfo serverInfo)
            {
                UpdateTasksCount++;
                RaisePropertyUpdate(IsUpdateStarted, nameof(IsUpdateStarted));

                return serverInfo.Update(5000, ct).ContinueWith(t =>
                {
                    UpdateTasksCount--;
                    RaisePropertyUpdate(IsUpdateStarted, nameof(IsUpdateStarted));

                    if (ct.IsCancellationRequested)
                        return false;

                    Dispatcher.UIThread.Invoke(() =>
                    {
                        Info = Info;

                        if (Page is FindServerPageViewModel page)
                        {
                            page.UpdateList();
                            page.Launcher?.SaveServerList();
                        }
                    });

                    if (ct.IsCancellationRequested)
                        return false;

                    return t.Result;
                });
            }

            return Task.FromResult(false);
        }

        protected override void OnPropertyChanged(object oldValue, object newValue, string name)
        {
            base.OnPropertyChanged(oldValue, newValue, name);
        }
    }
}

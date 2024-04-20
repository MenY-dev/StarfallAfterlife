using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Launcher.ViewModels
{
    public class RealmNameReportsViewModel : ViewModelBase
    {
        public ObservableCollection<ObjectNameReportsViewModel> Reports { get; } = new();

        public SfaRealmInfo RealmInfo { get; protected set; }

        public SfaServer Server { get; protected set; }

        public string RealmName => RealmInfo?.Realm?.Name;

        public RealmNameReportsViewModel() { }

        public RealmNameReportsViewModel(SfaServer server)
        {
            if (server is null)
                return;

            Server = server;
            RealmInfo = server.RealmInfo;
            UpdateReports();
        }

        public RealmNameReportsViewModel(SfaRealmInfo realm)
        {
            RealmInfo = realm;
            UpdateReports();
        }

        public void UpdateReports()
        {
            Reports.Clear();

            (Server?.RealmInfo ?? RealmInfo)?.Use(r =>
            {
                if (r.Realm?.Variable is SfaRealmVariable variable)
                {
                    var newReports = new Dictionary<RealmObjectNameReport, (bool IsSystem, bool IsPlanet)>();

                    if (variable.SystemNameReports is not null)
                    {
                        foreach (var item in variable.SystemNameReports.Values)
                            if (item is not null)
                                newReports[item] = (true, false);
                    }

                    if (variable.PlanetNameReports is not null)
                    {
                        foreach (var item in variable.PlanetNameReports.Values)
                            if (item is not null)
                                newReports[item] = (false, true);
                    }

                    var toRemove = Reports.Where(r => r?.ReportInfo is null || newReports.ContainsKey(r.ReportInfo) == false).ToArray();
                    var toAdd = newReports.Where(r => Reports.Any(vm => vm.ReportInfo == r.Key) == false).ToArray();

                    foreach (var item in toRemove)
                        Reports.Remove(item);

                    foreach (var item in Reports)
                    {
                        if (item is null)
                            continue;

                        var newAuthors = item.ReportInfo?.Authors;
                        var currentAuthors = item.Reports ??= new();

                        if (newAuthors is null)
                        {
                            currentAuthors?.Clear();
                            continue;
                        }

                        foreach (var author in currentAuthors)
                        {
                            if (newAuthors.Any(a => a.PlayerName == author) == false)
                                currentAuthors.Remove(author);
                        }

                        foreach (var author in newAuthors)
                        {
                            var name = author?.PlayerName;

                            if (currentAuthors.Contains(name) == false)
                                currentAuthors.Add(name);
                        }
                    }

                    foreach (var item in toAdd)
                    {
                        if (item.Key?.Id is int objectId)
                        {
                            var names = item.Value.IsSystem ? variable.RenamedSystems :
                                        item.Value.IsPlanet ? variable.RenamedPlanets : null;

                            var renameInfo = names?.GetValueOrDefault(objectId);

                            if (renameInfo is null)
                                continue;

                            Reports.Add(new()
                            {
                                Id = item.Key.Id,
                                IsSystem = item.Value.IsSystem,
                                IsPlanet = item.Value.IsPlanet,
                                Name = renameInfo.Name,
                                Author = renameInfo.Char,
                                Reports = new(item.Key.Authors?.Select(a => a.PlayerName) ?? Enumerable.Empty<string>()),
                            });
                        }
                    }
                }
                else
                {
                    Reports.Clear();
                }
            });
        }

        public void SubmitReport(object param) =>
            SubmitReport(param as ObjectNameReportsViewModel);

        public void SubmitReport(ObjectNameReportsViewModel report, bool autosave = true)
        {
            if (report is null)
                return;

            Reports.Remove(report);

            if (Server is not null)
            {
                Server.RealmInfo?.Use(r =>
                {
                    if (report.IsPlanet == true)
                    {
                        Server.RenamePlanet(report.Id, null, null);
                        r.Realm?.Variable?.PlanetNameReports?.Remove(report.Id);
                    }
                    else if (report.IsSystem == true)
                    {
                        Server.RenameSystem(report.Id, null, null);
                        r.Realm?.Variable?.SystemNameReports?.Remove(report.Id);
                    }

                    if (autosave == true)
                        r.SaveVariable();
                });
            }
            else if (RealmInfo is not null)
            {
                RealmInfo.Use(r =>
                {
                    if (r.Realm?.Variable is SfaRealmVariable variable)
                    {
                        if (report.IsPlanet == true)
                        {
                            variable.RenamedPlanets?.Remove(report.Id);
                            variable.PlanetNameReports?.Remove(report.Id);
                        }
                        else if (report.IsSystem == true)
                        {
                            variable.RenamedSystems?.Remove(report.Id);
                            variable.SystemNameReports?.Remove(report.Id);
                        }

                        if (autosave == true)
                            r.SaveVariable();
                    }
                });
            }
        }

        public void SubmitAllReports()
        {
            foreach (var item in Reports.ToArray())
                SubmitReport(item, false);

            (Server?.RealmInfo ?? RealmInfo)?.Use(r => r.SaveVariable());
        }

        public void RejectReport(object param) =>
            RejectReport(param as ObjectNameReportsViewModel);

        public void RejectReport(ObjectNameReportsViewModel report, bool autosave = true)
        {
            if (report is null)
                return;

            Reports.Remove(report);
            var realm = Server?.RealmInfo ?? RealmInfo;

            realm?.Use(r =>
            {
                if (report.IsPlanet == true)
                {
                    r.Realm?.Variable?.PlanetNameReports?.Remove(report.Id);
                }
                else if (report.IsSystem == true)
                {
                    r.Realm?.Variable?.SystemNameReports?.Remove(report.Id);
                }

                if (autosave == true)
                    r.SaveVariable();
            });
        }

        public void RejectAllReports()
        {
            foreach (var item in Reports.ToArray())
                RejectReport(item, false);

            (Server?.RealmInfo ?? RealmInfo)?.Use(r => r.SaveVariable());
        }
    }
}

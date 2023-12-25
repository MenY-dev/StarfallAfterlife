using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Realms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public partial class SfaLauncher
    {

        public List<SfaRealmInfo> Realms { get; } = new();

        public string RealmsDirectory => Path.Combine(WorkingDirectory, "Realms");

        private SfaRealmInfo _currentLocalRealm;

        protected List<SfaRealmInfo> LoadRealmsInfo()
        {
            List<SfaRealmInfo> result = new();

            try
            {
                DirectoryInfo[] directories =
                    GetOrCreateDirectory(RealmsDirectory)
                    ?.GetDirectories();

                foreach (var dir in directories)
                {
                    var info = new SfaRealmInfo()
                    {
                        RealmDirectory = dir.FullName,
                        Realm = new SfaRealm
                        {
                            Database = Database,
                            MobsDatabase = MobsDatabase.Instance,
                        }
                    };

                    if (info.LoadInfo() == true &&
                        info.Realm.Version <= SfaRealm.CurrentVersion)
                    {
                        result.Add(info);
                    }
                }

            }
            catch { }

            return result;
        }

        public SfaRealmInfo CreateNewRealm(string realmName)
        {
            try
            {
                var dir = FileHelpers.CreateUniqueDirectory(RealmsDirectory, "realm{0}");

                if (dir is null)
                    return null;

                var realm = new SfaRealmInfo()
                {
                    RealmDirectory = dir.FullName,
                    Realm = new SfaRealm
                    {
                        Name = realmName,
                        Id = Guid.NewGuid().ToString("N"),
                        Database = Database,
                        MobsDatabase = MobsDatabase.Instance,
                    }
                };

                realm.Save();
                Realms.Add(realm);
                return realm;
            }
            catch { }

            return null;
        }

        public void DeleteRealm(SfaRealmInfo realm)
        {
            try
            {
                Realms?.Remove(realm);

                if (CurrentLocalRealm == realm)
                    CurrentLocalRealm = null;

                realm.RemoveRealmFiles();
            }
            catch { }
        }
    }
}

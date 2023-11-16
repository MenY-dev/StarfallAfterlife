using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class VanillaRealmGenerator : GenerationTask
    {
        public SfaRealm Realm { get; }
        public SfaDatabase Database { get; }
        public MobsDatabase MobsDatabase { get; }

        public VanillaRealmGenerator(SfaRealm realm, SfaDatabase database = null, MobsDatabase mobsDatabase = null)
        {
            Realm = realm;
            Database = database ?? SfaDatabase.Instance;
            MobsDatabase = mobsDatabase ?? MobsDatabase.Instance;
        }

        protected override bool Generate()
        {
            if (Realm is null)
                return false;

            GenerationTask result;

            try
            {
                Realm.Database = Database;
                Realm.MobsDatabase = MobsDatabase;
                Realm.GalaxyMap = GalaxyMap.LoadDefaultMap();
                Realm.GalaxyMapHash = Realm.GalaxyMap.Hash;

                result = RunChildTasks(
                    new SecretObjectsGenerator(Realm),
                    new MobsMapGenerator(Realm),
                    new QuestsGenerator(Realm),
                    new SummarySeasonGenerator(Realm));
            }
            catch
            {
                return false;
            }

            return result?.Status == GenerationStatus.Success;
        }
    }
}

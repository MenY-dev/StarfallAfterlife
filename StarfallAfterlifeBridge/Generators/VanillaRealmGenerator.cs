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
        public int Seed { get; }
        public SfaRealm Realm { get; }
        public SfaDatabase Database { get; }
        public MobsDatabase MobsDatabase { get; }

        public VanillaRealmGenerator(SfaRealm realm, SfaDatabase database = null, MobsDatabase mobsDatabase = null, int seed = 0)
        {
            Realm = realm;
            Database = database ?? SfaDatabase.Instance;
            MobsDatabase = mobsDatabase ?? MobsDatabase.Instance;
            realm.Seed = Seed = seed;
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
                    new SecretObjectsGenerator(Realm, Seed),
                    new MobsMapGenerator(Realm, Seed),
                    new QuestsGenerator(Realm, Seed),
                    new SummarySeasonGenerator(Realm, Seed),
                    new BGShopGenerator(Realm, Seed));
            }
            catch
            {
                return false;
            }

            return result?.Status == GenerationStatus.Success;
        }
    }
}

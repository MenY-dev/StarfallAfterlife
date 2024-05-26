using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class MobsDatabase
    {
        public Dictionary<int, DiscoveryMobInfo> Mobs { get; } = new();

        protected Dictionary<int, List<DiscoveryMobInfo>> Circles { get; } = new();


        private static readonly Lazy<MobsDatabase> _lazyInstance =
            new(Load(Path.Combine("Database", "Mobs")));

        public static MobsDatabase Instance => _lazyInstance.Value;

        protected static MobsDatabase Load(string path)
        {
            var dtb = new MobsDatabase();

            try
            {
                if (Directory.Exists(path) == true &&
                    Directory.GetFiles(path) is string[] modsFiles)
                {
                    foreach (var file in modsFiles)
                    {
                        var text = File.ReadAllText(file);
                        var mob = JsonHelpers.DeserializeUnbuffered<DiscoveryMobInfo>(text);

                        if (mob is not null)
                        {
                            var accesLevel = SfaDatabase.LevelToAccessLevel(mob.Level);
                            dtb.Mobs.TryAdd(mob.Id, mob);

                            var circle = dtb.Circles.GetValueOrDefault(accesLevel);

                            if (circle is null)
                                circle = dtb.Circles[accesLevel] = new();

                            circle.Add(mob);
                        }
                    }
                }
            }
            catch { }

            return dtb;
        }

        public DiscoveryMobInfo GetMob(int id)
        {
            if (Mobs?.TryGetValue(id, out DiscoveryMobInfo mob) == true &&
                mob?.Id == id)
                return mob;

            return null;
        }

        public DiscoveryMobInfo GetMob(string name)
        {
            return Mobs?.Values?.FirstOrDefault(m => m.InternalName == name);
        }

        public DiscoveryMobInfo GetServiceFleet(Faction faction) =>
            GetMob(GetFactionServiceFleetName(faction));

        public IEnumerable<DiscoveryMobInfo> GetCircleMobs(int accellLevel)
        {
            return Circles?.GetValueOrDefault(accellLevel) ?? new();
        }

        public static string GetFactionServiceFleetName(Faction faction) => faction switch
        {
            Faction.Deprived => "dpr_disc_fleet",
            Faction.Eclipse => "ecl_disc_fleet",
            Faction.Vanguard => "vng_disc_fleet",
            Faction.Screechers => "screechers_fleets",
            Faction.Nebulords => "nebulords_fleet",
            Faction.Pyramid => "pyramid_fleets",
            Faction.MineworkerUnion => "miners_fleets",
            Faction.FreeTraders => "traders_fleet",
            _ => null,
        };
    }
}

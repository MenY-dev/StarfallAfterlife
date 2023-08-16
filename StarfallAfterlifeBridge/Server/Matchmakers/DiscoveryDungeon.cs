using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class DiscoveryDungeon : DiscoveryBattle
    {
        public override void Start()
        {
            InstanceInfo.Type = InstanceType.DiscoveryDungeon;
            InstanceInfo.DungeonFaction = SystemBattle?.DungeonInfo?.Target?.Faction ?? Faction.None;
            InstanceInfo.DungeonType = SystemBattle?.DungeonInfo?.Target.Type switch
            {
                DiscoveryObjectType.PiratesOutpost => DungeonType.Outpost,
                DiscoveryObjectType.PiratesStation => DungeonType.Station,
                _ => DungeonType.None,
            };

            CreateBosses();
            base.Start();
        }

        protected virtual void CreateBosses()
        {
            if (SystemBattle?.DungeonInfo.Target is StarSystemObject targetObjext && 
                Server.Realm.MobsMap.GetObjectMobs(SystemId, targetObjext.Type, targetObjext.Id) is List<GalaxyMapMob> mobs)
            {
                foreach (var mob in mobs)
                {
                    var info = CreateBossInfo(mob);

                    if (info is not null)
                        Bosses?.Add(info);
                }
            }

            //if (SystemBattle?.DungeonInfo.Target is StarSystemObject targetObjext &&
            //    Server?.Realm?.MobsDatabase is MobsDatabase database &&
            //    database.Mobs?.Values is IEnumerable<DiscoveryMobInfo> allMobs)
            //{
            //    var rnd = new Random();
            //    var accesLevel = targetObjext?.System.Info?.Level ?? 1;
            //    var minMobLevel = SfaDatabase.GetCircleMinLevel(accesLevel);
            //    var maxMobLevel = SfaDatabase.GetCircleMaxLevel(accesLevel);

            //    var bossesCount = targetObjext?.Type switch
            //    {
            //        DiscoveryObjectType.PiratesOutpost => 1 + accesLevel / 3,
            //        DiscoveryObjectType.PiratesStation => 3 + accesLevel / 2,
            //        _ => 0,
            //    };

            //    var circleBosses = allMobs
            //        .Where(m => m is not null && m.Level >= minMobLevel && m.Level <= maxMobLevel)
            //        .Where(m => m.Tags?.Contains("Mob.Role.Station", StringComparer.InvariantCultureIgnoreCase) ?? false)
            //        .Where(m => m.Tags?.Contains("Mob.Role.Boss", StringComparer.InvariantCultureIgnoreCase) ?? false);

            //    if (targetObjext is PiratesStation or PiratesOutpost)
            //    {
            //        circleBosses = circleBosses
            //            .Where(b => b.Faction == targetObjext.Faction);
            //    }

            //    var dungeonBosses = circleBosses.ToList();

            //    for (int i = 0; i < bossesCount; i++)
            //    {
            //        var bossIndex = rnd.Next() % dungeonBosses.Count;
            //        var info = CreateBossInfo(dungeonBosses[bossIndex]);

            //        if (info is not null)
            //            Bosses?.Add(info);
            //    }
            //}
        }

        protected override InstanceExtraData CreateExtraData()
        {
            var data = base.CreateExtraData() ?? new();

            if (SystemBattle?.DungeonInfo?.Target is StarSystemObject systemObject)
            {
                data.ParentObjType = (int)systemObject.Type;
                data.ParentObjId = systemObject.Id;
                data.ParentObjGroup = systemObject.FactionGroup;
                data.ParentObjLvl = systemObject.System?.Info?.Level ?? 1;
            }

            return data;
        }
    }
}

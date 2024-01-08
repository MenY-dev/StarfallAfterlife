using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Instances;
using StarfallAfterlife.Bridge.Networking;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Characters;
using StarfallAfterlife.Bridge.Server.Discovery;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StarfallAfterlife.Bridge.Server.Matchmakers
{
    public class DiscoveryDungeon : DiscoveryBattle
    {
        public override void Start()
        {
            lock (_lockher)
            {
                InstanceInfo.Type = InstanceType.DiscoveryDungeon;
                InstanceInfo.DungeonFaction = SystemBattle?.DungeonInfo?.Target?.Faction ?? Faction.None;
                InstanceInfo.DungeonType = SystemBattle?.DungeonInfo?.Target.Type switch
                {
                    DiscoveryObjectType.PiratesOutpost => DungeonType.Outpost,
                    DiscoveryObjectType.PiratesStation => DungeonType.Station,
                    DiscoveryObjectType.SecretObject => DungeonType.Secret,
                    _ => DungeonType.None,
                };

                CreateBosses();
                base.Start();
            }
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

                if (systemObject is SecretObject secret)
                {
                    data.AsteroidIntensity = 0.9f;
                }
            }

            return data;
        }

        protected override void CreateLocationDrop()
        {
            IEnumerable<SfaItem> GetLevelItems(int level) => Server?.Realm?.Database?
                .GetCircleDatabase(level)?.Equipments.Values
                .Where(i => i.IsAvailableForTrading == true) ??
                Enumerable.Empty<SfaItem>();

            if (SystemBattle.DungeonInfo?.Target is SecretObject secret &&
                secret.System is StarSystem system)
            {
                var lvl = system.Info?.Level ?? 1;

                if (secret.SecretType is SecretObjectType.Stash or SecretObjectType.ShipsGraveyard)
                {
                    var drop = new JsonArray();
                    var rnd = new Random128(secret.Id + system.Id);
                    var items = GetLevelItems(lvl);

                    if (lvl > 1)
                        items = items.Concat(GetLevelItems(lvl - 1));

                    items = items
                        .Where(i => i.IsDefective == false && i.TechLvl > 1)
                        .ToList().Randomize(rnd.Next());

                    foreach (var eq in items)
                    {
                        drop.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(eq.Id),
                            ["weight"] = SValue.Create(0.1f),
                            ["basecount"] = SValue.Create(1),
                        });
                    }

                    Drops["epic_" + lvl] = drop;
                }

                if (secret.SecretType is SecretObjectType.ShipsGraveyard)
                {
                    var drop = new JsonArray();
                    var rnd = new Random128(secret.Id + system.Id);
                    var items = GetLevelItems(lvl);

                    if (lvl > 1)
                        items = items.Concat(GetLevelItems(lvl - 1));

                    items = items
                        .Where(i => i.IsDefective == true)
                        .ToList().Randomize(rnd.Next());

                    foreach (var eq in items)
                    {
                        drop.Add(new JsonObject
                        {
                            ["id"] = SValue.Create(eq.Id),
                            ["weight"] = SValue.Create(0.25f),
                            ["basecount"] = SValue.Create(1),
                        });
                    }

                    Drops["relic_ship_level_" + lvl] = drop;
                }
            }
        }

        public override JsonNode GetMobData(MobDataRequest request)
        {
            lock (_lockher)
            {
                if (SystemBattle?.IsDungeon == true &&
                    SystemBattle.DungeonInfo?.Target is SecretObject secret &&
                    secret.System is StarSystem system &&
                    secret.SecretType == SecretObjectType.SpaceFarm &&
                    request.IsCustom == true)
                {
                    var tags = request.Tags?
                        .Where(t => "Mob.Role.Basic".Equals(t, StringComparison.InvariantCultureIgnoreCase) == false)
                        .ToArray() ?? Array.Empty<string>();

                    var mobs = (IEnumerable<DiscoveryMobInfo>)Server.Realm.MobsDatabase.Mobs.Values;

                    if (request.Faction != Faction.None)
                        mobs = mobs.Where(m => m.Faction == request.Faction);

                    if (tags.Length > 0)
                        mobs = mobs.Where(m => m.Tags?.All(t => m.Tags.Contains(t, StringComparer.InvariantCultureIgnoreCase)) == true);

                    if (request.MinLvl != 0 && request.MaxLvl != 0)
                        mobs = mobs.Where(m => m.Level >= request.MinLvl && m.Level <= request.MaxLvl);

                    var mob = mobs.FirstOrDefault();

                    if (mob is not null)
                    {
                        var ships = new JsonArray();
                        var shipId = 2000000000;

                        foreach (var ship in mob.Ships ?? Enumerable.Empty<DiscoveryMobShipData>())
                        {
                            if (ship?.Data is null)
                                continue;

                            var shipData = ship.Data.Clone();
                            shipData.Id = shipId++;

                            ships.Add(new JsonObject
                            {
                                ["id"] = SValue.Create(shipData.Id),
                                ["data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(shipData).ToJsonString(false)),
                                ["service_data"] = SValue.Create(JsonHelpers.ParseNodeUnbuffered(ship.ServiceData).ToJsonString(false)),
                            });
                        }

                        var doc = new JsonObject
                        {
                            ["id"] = SValue.Create(request.FleetId),
                            ["level"] = SValue.Create(mob.Level),
                            ["faction"] = SValue.Create((byte)mob.Faction),
                            ["internal_name"] = SValue.Create(mob.InternalName),
                            ["battle_bt"] = SValue.Create(mob.BehaviorTreeName),
                            ["tags"] = mob.Tags?.Select(SValue.Create).ToJsonArray(),
                            ["ships"] = ships,
                        };

                        return doc;
                    }
                }
            }

            return base.GetMobData(request);
        }

        protected override List<TileInfo> CreateLocationTiles()
        {
            if (SystemBattle.DungeonInfo?.Target is SecretObject secret &&
                secret.System is StarSystem system)
            {
                switch (secret.SecretType)
                {
                    case SecretObjectType.Stash:
                        return new()
                        {
                            new()
                            {
                                Name = "BP_TG_SecretStash_Screechers",
                                Remove = 0,
                                Priority = 200,
                                NumMin = 1,
                                NumMax = 1,
                                Params = new()
                                {
                                    ["secret_object_id"] = secret.Id,
                                },
                            }
                        };
                    case SecretObjectType.ShipsGraveyard:
                        return new()
                        {
                            new()
                            {
                                Name = "BP_TG_RelictShip",
                                Remove = 0,
                                Priority = 200,
                                NumMin = 1,
                                NumMax = 1,
                                Params = new()
                                {
                                    ["secret_object_id"] = secret.Id,
                                    ["quest_ident"] = -1,
                                },
                            },
                            new()
                            {
                                Name = "BP_TG_DebrisShip",
                                Remove = 0,
                                Priority = 150,
                                NumMin = 2,
                                NumMax = 3,
                                Params = new()
                                {
                                    ["secret_object_id"] = secret.Id,
                                },
                            },
                            new()
                            {
                                Name = "BP_TG_AbandonedShips",
                                Remove = 0,
                                Priority = 125,
                                NumMin = 2,
                                NumMax = 4,
                                Params = new()
                                {
                                    ["secret_object_id"] = secret.Id,
                                },
                            },
                            new()
                            {
                                Name = "BP_TG_Salvage",
                                Remove = 0,
                                Priority = 3,
                                NumMin = 5,
                                NumMax = 10,
                                Params = new(),
                            },
                            new()
                            {
                                Name = "BP_TG_RandomMovingAsteroids",
                                Remove = 0,
                                Priority = 4,
                                NumMin = 10,
                                NumMax = 20,
                                Params = new(),
                            },
                        };
                    case SecretObjectType.SpaceFarm:
                        return new()
                        {
                            new()
                            {
                                Name = "BP_TG_DefendFarmStation",
                                Remove = 0,
                                Priority = 200,
                                NumMin = 1,
                                NumMax = 1,
                                Params = new(),
                            },
                        };
                }
            }

            return null;
        }
    }
}

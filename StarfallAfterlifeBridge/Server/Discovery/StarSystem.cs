using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Realms;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Discovery
{
    public class StarSystem : DiscoveryObject
    {
        public GalaxyMapStarSystem Info { get; set; }

        public List<DiscoveryFleet> Fleets { get; } = new();

        public List<Planet> Planets { get; } = new();

        public List<DiscoveryMothership> Motherships { get; } = new();

        public List<WarpBeacon> WarpBeacons { get; } = new();

        public List<DiscoveryQuickTravelGate> QuickTravelGates { get; } = new();

        public List<PiratesStation> PiratesStations { get; } = new();

        public List<PiratesOutpost> PiratesOutposts { get; } = new();

        public List<MinerMothership> MinerMotherships { get; } = new();

        public List<ScienceStation> ScienceStations { get; } = new();

        public List<RepairStation> RepairStations { get; } = new();

        public List<FuelStation> FuelStation { get; } = new();

        public List<TradeStation> TradeStations { get; } = new();

        public List<StarSystemRichAsteroid> RichAsteroids { get; } = new();

        public List<SecretObject> SecretObjects { get; } = new();

        public NavigationMap NavigationMap { get; protected set; } = new();

        public SystemHexMap AsteroidsMap { get; protected set; } = new();

        public SystemHexMap NebulaMap { get; protected set; } = new();

        public SystemHexMap ObstacleMap { get; protected set; } = new();

        public List<StarSystemBattle> ActiveBattles { get; } = new();

        protected Dictionary<Action, DateTime> DefferedActions { get; } = new();

        public SfaDatabase Database { get; set; }

        public StarSystem() { }

        public StarSystem(DiscoveryGalaxy galaxy, int id)
        {
            Galaxy = galaxy;
            Id = id;
        }

        public override void Init()
        {
            base.Init();

            if (Galaxy is DiscoveryGalaxy galaxy &&
                galaxy.Map is GalaxyMap map)
            {
                Info = map.GetSystem(Id);
                Database = galaxy.Database;
            }

            if (Info is null)
                return;

            foreach (var item in Info.Planets ?? Enumerable.Empty<GalaxyMapPlanet>())
            {
                var planet = new Planet(item.Id);
                planet.Galaxy = Galaxy;
                planet.ParentObject = this;
                planet.Init();
                Planets.Add(planet);
            }

            foreach (var item in Info.Portals ?? Enumerable.Empty<GalaxyMapPortal>())
                WarpBeacons.Add(new WarpBeacon(item, this));

            foreach (var item in Info.Motherships ?? Enumerable.Empty<GalaxyMapMothership>())
                Motherships.Add(new DiscoveryMothership(item, this));

            foreach (var item in Info.QuickTravalGates ?? Enumerable.Empty<GalaxyMapQuickTravalGate>())
                QuickTravelGates.Add(new DiscoveryQuickTravelGate(item, this));

            foreach (var item in Info.PiratesStations ?? Enumerable.Empty<GalaxyMapPiratesStation>())
                PiratesStations.Add(new PiratesStation(item, this));

            foreach (var item in Info.PiratesOutposts ?? Enumerable.Empty<GalaxyMapPiratesOutpost>())
                PiratesOutposts.Add(new PiratesOutpost(item, this));

            foreach (var item in Info.MinerMotherships ?? Enumerable.Empty<GalaxyMapMinerMotherships>())
                MinerMotherships.Add(new MinerMothership(item, this));

            foreach (var item in Info.ScienceStations ?? Enumerable.Empty<GalaxyMapScienceStation>())
                ScienceStations.Add(new ScienceStation(item, this));

            foreach (var item in Info.RepairStations ?? Enumerable.Empty<GalaxyMapRepairStation>())
                RepairStations.Add(new RepairStation(item, this));

            foreach (var item in Info.FuelStations ?? Enumerable.Empty<GalaxyMapFuelStation>())
                FuelStation.Add(new FuelStation(item, this));

            foreach (var item in Info.TradeStations ?? Enumerable.Empty<GalaxyMapTradeStation>())
                TradeStations.Add(new TradeStation(item, this));

            foreach (var item in Info.RichAsteroids ?? Enumerable.Empty<GalaxyMapRichAsteroid>())
                RichAsteroids.Add(new StarSystemRichAsteroid(item, this));

            foreach (var item in Galaxy?.Realm?.SecretObjectsMap?.GetSystemObjects(Id) ?? Enumerable.Empty<SecretObjectInfo>())
                SecretObjects.Add(new SecretObject(item, this));

            UpdateNavigationMap();
            AsteroidsMap = new(Info.AsteroidsMask);
            NebulaMap = new(Info.NebulaMask);

            if (Galaxy?.Realm is SfaRealm realm &&
                realm?.MobsDatabase is MobsDatabase mobsDatabase &&
                realm?.MobsMap is MobsMap mobsMap &&
                mobsMap.GetSystemMobs(Id) is List<GalaxyMapMob> mobs &&
                mobs.Count > 0)
            {
                foreach (var mob in mobs)
                {
                    if (mob is not null &&
                        mobsDatabase.GetMob(mob.MobId) is DiscoveryMobInfo mobInfo)
                    {
                        if (mob.ObjectType != GalaxyMapObjectType.None ||
                            mob.ObjectId > -1)
                            continue;

                        var fleet = new DiscoveryAiFleet
                        {
                            Id = mob.FleetId,
                            Faction = mobInfo.Faction,
                            FactionGroup = mob.FactionGroup,
                            Name = mobInfo.InternalName,
                            Level = mobInfo.Level,
                            BaseSpeed = 4,
                            MobId = mobInfo.Id,
                            Hull = mobInfo.GetMainShipHull(),
                            Hex = mob.SpawnHex,
                        };

                        fleet.SetLocation(SystemHexMap.HexToSystemPoint(mob.SpawnHex), true);
                        fleet.SetAI(new MobAI());

                        AddFleet(fleet);
                    }
                }
            }
        }

        public virtual void Update()
        {
            UpdateFleets();
            UpdateDefferedActions();
        }

        protected virtual void UpdateFleets()
        {
            foreach (var fleet in Fleets)
                fleet?.Update();
        }

        public virtual void UpdateNavigationMap()
        {
            var starSize = Info?.Size / 150f ?? 1;
            List<Vector2> centers = new();
            List<float> radiuses = new();
            var objects = Enumerable.Empty<StarSystemObject>()
                .Concat(Planets)
                .Concat(PiratesOutposts)
                .Concat(PiratesStations)
                .Concat(MinerMotherships)
                .Concat(ScienceStations)
                .Concat(RepairStations)
                .Concat(FuelStation)
                .Concat(TradeStations)
                .Concat(Motherships)
                .Concat(QuickTravelGates)
                .Concat(RichAsteroids);

            centers.Add(Vector2.Zero);
            radiuses.Add(starSize);

            ObstacleMap = new SystemHexMap(h => SystemHexMap.HexToSystemPoint(h).GetSize() < starSize);

            foreach (var obj in objects)
            {
                centers.Add(obj.Location);
                radiuses.Add(GetObjectRadius(obj));
            }

            NavigationMap = NavigationMap.Create(centers, radiuses);
        }

        public float GetObjectRadius(StarSystemObject systemObject) => systemObject switch
        {
            null => 0,
            Planet planet => planet.Size / 400,
            DiscoveryMothership or PiratesStation => 2.5f,
            PiratesOutpost or ScienceStation or RepairStation or TradeStation => 1.75f,
            _ => 1f
        };

        public void AddFleet(DiscoveryFleet fleet, Vector2? location = null)
        {
            if (fleet is null || fleet.Id < 0)
                return;

            StarSystem oldSystem = fleet.System;

            if (oldSystem != this)
                oldSystem?.RemoveFleet(fleet);

            if (Fleets.Contains(fleet) == false)
            {
                if (location is not null)
                    fleet.SetLocation(location.Value);

                Fleets.Add(fleet);
            }

            fleet.ParentObject = this;

            if (GetFleetBattle(fleet) is null)
                fleet.SetFleetState(FleetState.InGalaxy);

            Broadcast<IStarSystemObjectListener>(l => l.OnObjectSpawned(fleet));
        }

        public void RemoveFleet(DiscoveryFleet fleet)
        {
            if (fleet?.System != this)
                return;

            Fleets.Remove(fleet);
            fleet.ParentObject = null;
            Broadcast<IStarSystemObjectListener>(l => l.OnObjectDestroed(fleet));
        }

        public void AddDeferredAction(Action action, TimeSpan waitingTime) =>
            AddDeferredAction(action, DateTime.Now + waitingTime);

        public void AddDeferredAction(Action action, DateTime invokeTime)
        {
            if (action is null)
                return;

            DefferedActions[action] = invokeTime;
        }

        public void UpdateDefferedActions()
        {
            var now = DateTime.Now;
            var toRemove = new List<Action>();

            foreach (var action in DefferedActions)
            {
                if (action.Value <= now)
                {
                    toRemove.Add(action.Key);
                    action.Key.Invoke();
                }
            }

            foreach (var action in toRemove)
                DefferedActions.Remove(action);
        }

        public virtual SystemHex GetNearestSafeHex(DiscoveryFleet fleet, SystemHex targetHex, bool ignoreTargetHex = true)
        {
            if (fleet is null)
                return targetHex;

            var closedHexes = new List<SystemHex>();
            var myFaction = fleet.Faction;
            var hexes = targetHex.GetSpiralEnumerator(33);

            if (ignoreTargetHex == false)
                hexes = Enumerable.Repeat(targetHex, 1).Concat(hexes);

            closedHexes.AddRange(ActiveBattles.Select(s => s.Hex));
            closedHexes.AddRange(GetAllObjects(false).Select(s => s.Hex));
            closedHexes.AddRange(Fleets.Where(f => f.Faction.IsEnemy(myFaction)).Select(s => s.Hex));

            foreach (var hex in hexes)
            {
                if (hex.GetSize() > 16 || ObstacleMap[hex] == true)
                    continue;

                if (closedHexes.Contains(hex) == false)
                    return hex;
            }

            return targetHex;
        }

        public virtual StarSystemBattle GetFleetBattle(DiscoveryFleet fleet) =>
            ActiveBattles?.FirstOrDefault(b => b?.GetMember(fleet) is not null);

        public void AddBattle(StarSystemBattle battle)
        {
            if (battle is null || ActiveBattles.Contains(battle) == true)
                return;

            if (battle.System != this)
                battle.System = this;

            ActiveBattles.Add(battle);
        }

        public void RemoveBattle(StarSystemBattle battle)
        {
            ActiveBattles.Remove(battle);
        }

        public StarSystemBattle GetBattle(Guid id) =>
            ActiveBattles.FirstOrDefault(b => b.Id == id);

        public StarSystemBattle GetBattle(StarSystemObject systemObject) =>
            ActiveBattles.FirstOrDefault(b => b?.IsInBattle(systemObject) == true);

        public override void Broadcast<TListener>(Action<TListener> action)
        {
            base.Broadcast(action);

            foreach (var item in GetAllObjects(true))
                item?.Listeners.Broadcast(action);
        }

        public override void Broadcast<TListener>(Action<TListener> action, Func<TListener, bool> predicate)
        {
            base.Broadcast(action, predicate);

            foreach (var item in GetAllObjects(true))
                item?.Listeners.Broadcast(action, predicate);
        }

        public virtual IEnumerable<StarSystemObject> GetObjectsAt(SystemHex hex, bool includeFleets = false)
        {
            foreach (var item in GetAllObjects(includeFleets))
            {
                if (item is null || item.Hex != hex)
                    continue;

                yield return item;
            }
        }

        public virtual IEnumerable<T> GetObjectsAt<T>(SystemHex hex, bool includeFleets = false) where T : StarSystemObject
        {
            foreach (var item in GetAllObjects(includeFleets))
            {
                if (item is not T || item.Hex != hex)
                    continue;

                yield return item as T;
            }
        }

        public virtual StarSystemObject GetObjectAt(SystemHex hex, bool includeFleets = false)
        {
            return GetObjectsAt(hex, includeFleets)?.FirstOrDefault();
        }

        public virtual StarSystemObject GetObjectAt(SystemHex hex, DiscoveryObjectType objectType, bool includeFleets = false)
        {
            return GetObjectsAt(hex, includeFleets)?.FirstOrDefault(o => o.Type == objectType);
        }

        public virtual IEnumerable<StarSystemObject> GetAllObjects(bool includeFleets = false)
        {
            var objects = Enumerable.Empty<StarSystemObject>()
                .Concat(Planets)
                .Concat(WarpBeacons)
                .Concat(PiratesOutposts)
                .Concat(PiratesStations)
                .Concat(MinerMotherships)
                .Concat(ScienceStations)
                .Concat(RepairStations)
                .Concat(FuelStation)
                .Concat(TradeStations)
                .Concat(Motherships)
                .Concat(QuickTravelGates)
                .Concat(SecretObjects)
                .Concat(RichAsteroids);

            return includeFleets == true ? objects.Concat(Fleets) : objects;
        }

        public virtual StarSystemObject GetObject(int id) =>
            GetAllObjects(true).FirstOrDefault(o => o.Id == id);

        public virtual StarSystemObject GetObject(int id, DiscoveryObjectType type) =>
            GetAllObjects(true).FirstOrDefault(o => o.Id == id && o.Type == type);

        public virtual T GetObject<T>(int id) where T : StarSystemObject =>
            GetAllObjects(true)
            .Select(o => o as T)
            .FirstOrDefault(o => o is not null && o.Id == id);

        public virtual T GetObject<T>(int id, DiscoveryObjectType type) where T : StarSystemObject =>
            GetAllObjects(true)
            .Select(o => o as T)
            .FirstOrDefault(o => o is not null && o.Id == id && o.Type == type);
    }
}

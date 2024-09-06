using StarfallAfterlife.Bridge.Collections;
using StarfallAfterlife.Bridge.Database;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Generators
{
    public class DefaultGalaxyMapGenerator
    {
        public static GalaxyMap Build()
        {
            string mapName = "DefaultMap";
            var map = JsonHelpers
                .DeserializeUnbuffered<GalaxyMap>(File
                .ReadAllText(Path
                .Combine("Database", "NewRealm.json")));

            GenerateRichAsteroids(map, 0);
            GeneratePiratesOutposts(map, 0);
            map.Hash = mapName;

            File.WriteAllText(
                Path.Combine("V:", $"{mapName}.json"),
                JsonHelpers.SerializeUnbuffered(map, new (){ WriteIndented = true, TypeInfoResolver = JsonSerializerOptions.Default.TypeInfoResolver }));

            return map;
        }

        public static void GeneratePiratesOutposts(GalaxyMap map, int seed = 0)
        {
            var rnd = new Random128(seed);
            int id = 0;

            foreach (var system in map.Systems)
            {
                if (((Faction)system.Faction).IsPirates() == false)
                    continue;

                int chance = system.Level switch
                { 
                    > 6 => 1,
                    > 5 => 2,
                    > 4 => 3,
                    _ => 4,
                };

                if (system.PiratesStations?.Any() == true)
                    chance += 2;

                if ((rnd.Next() % chance) > 2)
                    continue;

                var outpost = new GalaxyMapPiratesOutpost
                {
                    Faction = system.Faction,
                    FactionGroup = system.FactionGroup,
                    Level = system.Level,
                    Id = id,
                    X = 0,
                    Y = 0,
                };

                bool isSucces = false;
                var posRnd = new Random128(id);
                var asteroids = new SystemHexMap(system.AsteroidsMask);
                var nebulas = new SystemHexMap(system.NebulaMask);
                var hexes = Enumerable
                    .Range(0, SystemHexMap.HexesCount)
                    .ToList()
                    .Randomize(id);

                foreach (var item in hexes)
                {
                    if (nebulas[item] == true)
                        continue;

                    var hex = SystemHexMap.ArrayIndexToHex(item);
                    var isAvailable = hex
                        .GetSpiralEnumerator(2)
                        .Any(h => 
                        system.GetObjectAt(h.X, h.Y) is not null ||
                        asteroids[h] == true) == false;

                    if (isAvailable == true)
                    {
                        isSucces = true;
                        outpost.X = hex.X;
                        outpost.Y = hex.Y;
                        break;
                    }
                }

                if (isSucces == false)
                    continue;

                id++;
                (system.PiratesOutposts ??= new()).Add(outpost);
            }
        }


        public static void GenerateRichAsteroids(GalaxyMap map, int seed = 0)
        {
            var rnd = new Random128(seed);
            int id = 0;
            var ores = SfaDatabase.Instance.DiscoveryItems?.Values
                .Where(i => i.Tags.Contains("Item.Role.Ore") == true).ToList() ?? new();

            foreach (var system in map.Systems)
            {
                var faction = (Faction)system.Faction;

                if (faction.IsMainFaction() == true)
                    continue;

                int chance = (Faction)system.Faction switch
                {
                    Faction.MineworkerUnion => 2,
                    Faction.None => 18,
                    _ => 4,
                };

                var hexes = new SystemHexMap(system.AsteroidsMask).ToList();
                var asteroids = new List<GalaxyMapRichAsteroid>();

                for (int i = 0; i < hexes.Count; i++)
                {
                    if (hexes[i] == false || (rnd.Next() % chance) != 0)
                        continue;

                    var position = SystemHexMap.ArrayIndexToHex(i);
                    var field = new GalaxyMapRichAsteroid
                    {
                        Id = id,
                        X = position.X,
                        Y = position.Y,
                        Ores = new(),
                    };

                    foreach (var ore in ores)
                    {
                        if (system.Level < ore.MinLvl || system.Level > ore.MaxLvl)
                            continue;

                        field.Ores.Add(ore.Id);
                    }

                    if (field.Ores.Count > 0)
                    {
                        asteroids.Add(field);
                        id++;
                    }
                }

                if (asteroids.Any() == true)
                    system.RichAsteroids = asteroids;
            }
        }
    }
}

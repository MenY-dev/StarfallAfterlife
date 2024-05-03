using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Houses
{
    public class SfHouseDatabase
    {
        public string DatabaseDirectory { get; set; }

        public Dictionary<int, SfHouseInfo> Houses { get; } = new();

        private HouseCreationResult AddHouse(SfHouseInfo houseInfo)
        {
            var house = houseInfo?.House;

            if (house is null)
                return HouseCreationResult.Unknown;

            if (house.Guid == Guid.Empty)
                house.Guid = Guid.NewGuid();

            if (Houses.Any(h => h.Value?.House?.Guid == house.Guid) == true)
                return HouseCreationResult.NameAlreadyExists;

            try
            {
                if (DatabaseDirectory is string dtbDir &&
                    (houseInfo.Location is null ||
                    houseInfo.Location.StartsWith(DatabaseDirectory) == false))
                {
                    var fileValidName = FileHelpers.ReplaceInvalidFileNameChars(house.Name, '_');
                    var fileName = $"{fileValidName}_{house.Guid:N}.json";
                    houseInfo.Location = Path.Combine(dtbDir, fileName);
                }

                house.Id = Houses.Count < 1 ? 1 : Houses.Max(h => h.Value?.House?.Id ?? 0) + 1;
                Houses[house.Id] = houseInfo;
            }
            catch { return HouseCreationResult.Unknown; }

            return HouseCreationResult.Success;
        }

        public HouseCreationResult CreateHouse(string name, string tag, out SfHouseInfo houseInfo)
        {
            houseInfo = null;

            if (string.IsNullOrWhiteSpace(name) == true)
                return HouseCreationResult.NameIsTooLong;

            if (string.IsNullOrWhiteSpace(tag) == true)
                return HouseCreationResult.TagIsTooLong;

            foreach (var house in Houses.Select(h => h.Value?.House))
            {
                if (house is null)
                    continue;

                if (name.Equals(house.Name, StringComparison.OrdinalIgnoreCase) == true)
                    return HouseCreationResult.NameAlreadyExists;

                if (tag.Equals(house.Tag, StringComparison.OrdinalIgnoreCase) == true)
                    return HouseCreationResult.TagAlreadyExists;
            }

            houseInfo = new SfHouseInfo()
            {
                House = new SfHouse()
                {
                    Tag = tag,
                    Name = name,
                    Guid = Guid.NewGuid(),
                    Level = 20,
                    MaxCurrency = 10000,
                    Currency = 9000,
                    MaxMembers = 100,
                    Xp = 1546091,
                    TasksPoolSize = 70,
                    DoctrineAccessLevels = { 1, 2, 3 },
                    Tasks =
                    {
                        { "house_task_kill_mob", 70 },
                        { "house_task_scan_unknown_planet", 70 },
                        { "house_task_kill_boss", 70 },
                    }
                },
            };

            var result = AddHouse(houseInfo);

            if (result != HouseCreationResult.Success)
                houseInfo = null;

            return result;
        }

        public bool Load(string path)
        {
            try
            {
                DatabaseDirectory = path;
                Houses.Clear();

                if (FileHelpers.EnumerateFilesSelf(path).ToArray() is string[] houseFiles)
                {
                    foreach (var file in houseFiles)
                    {
                        try
                        {
                            var text = File.ReadAllText(file);
                            var house = new SfHouseInfo() { Location = file, };

                            if (house.Load() == true)
                                AddHouse(house);
                        }
                        catch { }
                    }

                    return true;
                }
            }
            catch { }

            return false;
        }

        public SfHouseInfo GetHouse(Guid playerId, Guid charId)
        {
            return Houses.Values.FirstOrDefault(
                h => h?.House?.Members?.Values.Any(
                    m => m is not null && m.PlayerId == playerId && m.CharacterId == charId) == true);
        }

        public void DeleteHouse(SfHouseInfo houseInfo)
        {
            if (houseInfo is null)
                return;

            var houseKey = Houses.FirstOrDefault(h => h.Value == houseInfo, new(-1, null)).Key;

            if (houseKey > -1)
                Houses.Remove(houseKey);

            try
            {
                if (string.IsNullOrWhiteSpace(houseInfo.Location) == false)
                    File.Delete(houseInfo.Location);
            }
            catch { }
        }

        public void DeleteHouse(SfHouse house)
        {
            if (house is null)
                return;

            var houseEntry = Houses.FirstOrDefault(h => h.Value?.House == house, new(-1, null));
            var houseInfo = houseEntry.Value;

            if (houseEntry.Key > -1)
                Houses.Remove(houseEntry.Key);

            if (houseInfo is null)
                return;

            try
            {
                if (string.IsNullOrWhiteSpace(houseInfo.Location) == false)
                    File.Delete(houseInfo.Location);
            }
            catch { }
        }
    }
}

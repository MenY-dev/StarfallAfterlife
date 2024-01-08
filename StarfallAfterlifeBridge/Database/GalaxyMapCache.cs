using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server.Galaxy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Database
{
    public class GalaxyMapCache
    {
        public string Location { get; set; }

        private readonly object _lockher = new();
        private const string _mapFileName = "galaxy_map.json";
        private const string _infoFileName = "map_info.json";
        private readonly Dictionary<string, (string Directory, DateTime LastLoad)> _maps = new();

        public void Init()
        {
            lock (_lockher)
            {
                try
                {
                    _maps.Clear();

                    var dtbDir = Location;

                    if (dtbDir is null ||
                        Directory.Exists(dtbDir) == false)
                        return;

                    var subDirs = Directory.GetDirectories(dtbDir);

                    foreach (var dir in subDirs)
                    {
                        var infoPath = Path.Combine(dir, _infoFileName);
                        var info = JsonHelpers.ParseNodeUnbuffered(File.ReadAllText(infoPath));

                        if ((string)info["map_hash"] is string hash)
                        {
                            var lastLoad = info["last_load"]?.DeserializeUnbuffered<DateTime>();
                            var needUpdateLastLoad = lastLoad is null;
                            _maps[hash] = (dir, lastLoad.Value);

                            if (needUpdateLastLoad == true)
                                UpdateLastLoad(hash);
                        }
                    }
                }
                catch { }
            }
            
            ClearTrash();
        }

        public GalaxyMap Load(string hash)
        {
            lock (_lockher)
            {
                string text = LoadText(hash);

                if (text is null)
                    return null;

                try
                {
                    return JsonHelpers.DeserializeUnbuffered<GalaxyMap>(text);
                }
                catch { }

                return null;
            }
        }

        public string LoadText(string hash)
        {
            lock (_lockher)
            {
                var info = _maps.GetValueOrDefault(hash);

                if (info.Directory is null)
                    return null;

                try
                {
                    var path = Path.Combine(info.Directory, _mapFileName);

                    if (File.Exists(path) == false)
                        return null;

                    var text = File.ReadAllText(path);
                    UpdateLastLoad(hash);

                    return text;
                }
                catch
                {
                    return null;
                }
            }
        }

        public void Save(GalaxyMap map)
        {
            if (map is null ||
                string.IsNullOrWhiteSpace(map.Hash) == true)
                return;

            lock (_lockher)
            {
                if (JsonHelpers.SerializeUnbuffered(map) is string mapText)
                    Save(map.Hash, mapText);
            }
        }

        public void Save(string hash, string mapText)
        {
            if (hash is null || mapText is null)
                return;

            lock (_lockher)
            {
                try
                {
                    var dir = _maps.GetValueOrDefault(hash).Directory;

                    if (dir is null ||
                        File.Exists(dir) == false)
                    {
                        dir = Path.Combine(Location, CreateDirectoryName(hash));
                        Directory.CreateDirectory(dir);
                    }

                    File.WriteAllText(Path.Combine(dir, _mapFileName), mapText);

                    _maps[hash] = (dir, DateTime.Now);
                    SaveInfo(hash);
                }
                catch { }
            }

            ClearTrash();
        }

        public void Remove(string hash)
        {
            lock (_lockher)
            {
                var info = _maps.GetValueOrDefault(hash);
                _maps.Remove(hash);

                if (info.Directory is null)
                    return;

                try
                {
                    if (Directory.Exists(info.Directory) == true)
                        Directory.Delete(info.Directory, true);
                }
                catch { }
            }

            ClearTrash();
        }

        public void ClearTrash()
        {
            lock (_lockher)
            {
                var toRemove = new List<string>();

                foreach (var item in _maps)
                {
                    var info = item.Value;

                    try
                    {
                        if (info.Directory is null ||
                            File.Exists(Path.Combine(info.Directory, _mapFileName)) == false)
                            toRemove.Add(item.Key);
                    }
                    catch { }
                }

                foreach (var item in toRemove)
                {
                    try
                    {
                        _maps.Remove(item);

                        if (_maps.GetValueOrDefault(item).Directory is string dir &&
                            Directory.Exists(dir))
                            Directory.Delete(dir, true);
                    }
                    catch { }
                }
            }
        }

        public void UpdateLastLoad(string hash)
        {
            lock (_lockher)
            {
                var info = _maps.GetValueOrDefault(hash);

                if (info.Directory is null)
                    return;

                info.LastLoad = DateTime.Now;
                _maps[hash] = info;
                SaveInfo(hash);
            }
        }

        protected void SaveInfo(string hash)
        {
            lock (_lockher)
            {
                var info = _maps.GetValueOrDefault(hash);

                if (info.Directory is null)
                    return;

                try
                {
                    if (Directory.Exists(info.Directory) == false)
                        Directory.CreateDirectory(info.Directory);

                    var infoPath = Path.Combine(info.Directory, _infoFileName);

                    File.WriteAllText(infoPath, new JsonObject
                    {
                        ["map_hash"] = hash,
                        ["last_load"] = JsonHelpers.ParseNodeUnbuffered(info.LastLoad),
                    }.ToJsonStringUnbuffered(false));
                }
                catch { }
            }
        }

        protected string CreateDirectoryName(string hash)
        {
            if (hash is null)
                return null;

            var name = hash.ToLowerInvariant();

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '-');

            if (name.Length > 22)
                name = name[..22];

            name = $"{name}-{new Random128().Next():D}";

            return name ;
        }
    }
}

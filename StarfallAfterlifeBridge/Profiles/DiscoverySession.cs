using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace StarfallAfterlife.Bridge.Profiles
{
    public class DiscoverySession : SfaObject
    {
        public string RealmId { get; set; }

        public int CharacterId { get; set; }

        public int SessionStartIGC { get; set; }

        public int SessionStartBGC { get; set; }

        public int SessionStartXp { get; set; }

        public int SessionStartPP { get; set; }

        public int SessionStartAccessLevel { get; set; }

        public int SystemId { get; set; }

        public Vector2 Location { get; set; }

        public List<ShipConstructionInfo> Ships { get; set; } = new();

        public List<InventoryItem> SessionStartInventory { get; set; } = new();

        public Dictionary<int, int> StartHullXps { get; set; } = new();

        public Dictionary<int, int> StartShipsXps { get; set; } = new();

        public DateTime SessionStartTime { get; set; }

        public DateTime LastUpdate { get; set; }

        public string Path { get; set; }

        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is not JsonObject)
                return;

            RealmId = (string)doc["realm_id"];
            CharacterId = (int?)doc["char_id"] ?? 0;
            SessionStartIGC = (int?)doc["start_igc"] ?? 0;
            SessionStartBGC = (int?)doc["start_bgc"] ?? 0;
            SessionStartXp = (int?)doc["start_xp"] ?? 0;
            SessionStartPP = (int?)doc["start_pp"] ?? 0;
            SessionStartAccessLevel = (int?)doc["start_lvl"] ?? 0;
            SystemId = (int?)doc["system_id"] ?? 0;
            Location = doc["location"]?.DeserializeUnbuffered<Vector2>() ?? Vector2.Zero;
            SessionStartTime = doc["session_start_time"].GetValue<DateTime>();
            LastUpdate = doc["last_update_time"].GetValue<DateTime>();
            SessionStartInventory = doc["session_start_inventory"]?.DeserializeUnbuffered<List<InventoryItem>>() ?? new();
            StartHullXps = doc["start_hull_xps"]?.DeserializeUnbuffered<Dictionary<int, int>>() ?? new();
            StartShipsXps = doc["start_ships_xps"]?.DeserializeUnbuffered<Dictionary<int, int>>() ?? new();

            (Ships ??= new()).Clear();

            if (doc["ships"]?.AsArray() is JsonArray ships)
                Ships.AddRange(ships.DeserializeUnbuffered<List<ShipConstructionInfo>>() ?? new());
        }

        public override JsonNode ToJson()
        {
            var doc = new JsonObject();
            var ships = JsonHelpers.ParseNodeUnbuffered(Ships ?? new()) ?? new JsonArray();

            doc["realm_id"] = RealmId;
            doc["char_id"] = CharacterId;
            doc["start_igc"] = SessionStartIGC;
            doc["start_bgc"] = SessionStartBGC;
            doc["start_xp"] = SessionStartXp;
            doc["start_pp"] = SessionStartPP;
            doc["start_lvl"] = SessionStartAccessLevel;
            doc["system_id"] = SystemId;
            doc["location"] = JsonHelpers.ParseNodeUnbuffered(Location);
            doc["session_start_time"] = SessionStartTime;
            doc["last_update_time"] = LastUpdate;
            doc["session_start_inventory"] = JsonHelpers.ParseNodeUnbuffered(SessionStartInventory);
            doc["start_hull_xps"] = JsonHelpers.ParseNodeUnbuffered(StartHullXps);
            doc["start_ships_xps"] = JsonHelpers.ParseNodeUnbuffered(StartShipsXps);
            doc["ships"] = ships;

            return doc;
        }

        public bool Load()
        {
            try
            {
                if (File.Exists(Path) == false)
                    return false;

                var text = File.ReadAllText(Path);
                var doc = JsonNode.Parse(text);
                LoadFromJson(doc);
                return true;
            }
            catch { }

            return false;
        }

        public bool Save()
        {
            try
            {
                if (System.IO.Path.GetDirectoryName(Path) is string dir &&
                    Directory.Exists(dir) == false)
                    Directory.CreateDirectory(dir);

                var doc = ToJson();
                var text = doc.ToJsonString(false);
                File.WriteAllText(Path, text);
                return true;
            }
            catch { }

            return false;
        }

        public bool RemoveSessionFile()
        {
            try
            {
                File.Delete(Path);
                return true;
            }
            catch { }

            return false;
        }
    }
}
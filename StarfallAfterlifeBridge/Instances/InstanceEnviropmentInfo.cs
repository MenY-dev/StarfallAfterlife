using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceEnviropmentInfo : SfaObject
    {
        public int SystemLevel { get; set; } = 0;
        public int StarId { get; set; } = 0;
        public int StarSize{ get; set; } = 0;
        public int StarWeight { get; set; } = 0;
        public int StarTemp { get; set; } = 0;
        public int StarType { get; set; } = 0;
        public int PosX { get; set; } = 0;
        public int PosY { get; set; } = 0;
        public bool HasNebula { get; set; } = false;
        public float DistanceFactor { get; set; } = 1;
        public int DistanceFromCenter { get; set; } = 0;
        public int SectorLockSeconds { get; set; } = 0;
        public int FloatingAsteroidsCount { get; set; } = 0;
        public int RichAsteroidsId { get; set; } = -1;
        public int RichAsteroidsType { get; set; } = 0;
        public Dictionary<int, int> AsteroidsContent { get; protected set; } = new();

        public List<string> EnviropmentEffects { get; set; }

        public override JsonNode ToJson()
        {
            var doc = base.ToJson() ?? new JsonObject();

            doc["system_level"] = SystemLevel;
            doc["star_id"] = StarId;
            doc["star_size"] = StarSize;
            doc["star_weight"] = StarWeight;
            doc["star_temp"] = StarTemp;
            doc["star_type"] = StarType;
            doc["pos_x"] = PosX;
            doc["pos_y"] = PosY;
            doc["has_nebula"] = HasNebula;
            doc["distance_factor"] = DistanceFactor;
            doc["distance_from_center"] = DistanceFromCenter;
            doc["sector_lock_seconds"] = SectorLockSeconds;
            doc["env_effects"] = EnviropmentEffects?.Select(e => new JsonObject { ["type"] = e }).ToJsonArray();
            doc["floating_asteroids_count"] = FloatingAsteroidsCount;

            if (RichAsteroidsId != -1)
            {
                doc["rich_asteroids_id"] = RichAsteroidsId;
                doc["rich_asteroids_type"] = RichAsteroidsType;
                doc["asteroids_content"] = AsteroidsContent.Select(a => new JsonObject
                {
                    ["entity"] = a.Key,
                    ["count"] = a.Value,
                }).ToJsonArray();
            }

            return doc;
        }

        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is not JsonObject)
                return;

            SystemLevel = (int?)doc["system_level"] ?? 0;
            StarId = (int?)doc["star_id"] ?? 0;
            StarSize = (int?)doc["star_size"] ?? 0;
            StarWeight = (int?)doc["star_weight"] ?? 0;
            StarTemp = (int?)doc["star_temp"] ?? 0;
            StarType = (int?)doc["star_type"] ?? 0;
            PosX = (int?)doc["pos_x"] ?? 0;
            PosY = (int?)doc["pos_y"] ?? 0;
            HasNebula = (bool?)doc["has_nebula"] ?? false;
            DistanceFactor = (float?)doc["distance_factor"] ?? 1;
            DistanceFromCenter = (int?)doc["distance_from_center"] ?? 0;
            SectorLockSeconds = (int?)doc["sector_lock_seconds"] ?? 0;
            FloatingAsteroidsCount = (int?)doc["floating_asteroids_count"] ?? 0;
            RichAsteroidsId = (int?)doc["rich_asteroids_id"] ?? 0;
            RichAsteroidsType = (int?)doc["rich_asteroids_type"] ?? 0;
            AsteroidsContent = new();

            if (doc["asteroids_content"] is JsonArray asteroidsContent)
                foreach (var item in asteroidsContent)
                    if (item is JsonObject jo &&
                        (int?)jo["entity"] is int entity &&
                        (int?)jo["count"] is int count)
                        AsteroidsContent[entity] = count;

            EnviropmentEffects = new();

            if (doc["env_effects"] is JsonArray effects)
                foreach (var item in effects)
                    if ((string)item["type"] is string type)
                        EnviropmentEffects.Add(type);
        }
    }
}

﻿using StarfallAfterlife.Bridge.Primitives;
using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceExtraData : SfaObject
    {
        public int? NoPlayersLifetime { get; set; }

        public int? LevelBoxSize { get; set; }

        public int ParentObjId { get; set; } = 0;

        public int ParentObjType { get; set; } = 0;

        public int ParentObjGroup { get; set; } = 0;

        public int ParentObjLvl { get; set; } = 0;

        public int SpecOpsDifficulty { get; set; } = 0;

        public float? AsteroidIntensity { get; set; }

        public InstanceEnviropmentInfo EnviropmentInfo { get; set; }

        public InstanceXpData XpData { get; set; } = new();

        public List<InstanceAIFleet> AiList { get; set; } = new();

        public List<InstanceMob> Bosses { get; set; } = new();

        public List<TileInfo> Tiles { get; set; } = null;

        public override JsonNode ToJson()
        {
            var doc = base.ToJson() ?? new JsonObject();
            var aiList = new JsonArray();
            var bosses = new JsonArray();

            foreach (var ai in AiList)
            {
                var aiNode = JsonHelpers.ParseNodeUnbuffered(ai);

                if (aiNode is not null)
                    aiList.Add(aiNode);
            }

            foreach (var boss in Bosses)
            {
                var aiNode = JsonHelpers.ParseNodeUnbuffered(boss);

                if (aiNode is not null)
                    bosses.Add(aiNode);
            }

            doc["instance_parent_obj_id"] = ParentObjId;
            doc["instance_parent_obj_type"] = ParentObjType;
            doc["instance_parent_obj_group"] = ParentObjGroup;
            doc["instance_parent_obj_lvl"] = ParentObjLvl;
            doc["spec_ops_difficulty"] = SpecOpsDifficulty;

            if (AsteroidIntensity is not null)
                doc["AsteroidIntensity"] = (float)AsteroidIntensity;

            if (NoPlayersLifetime is not null)
                doc["no_players_lifetime"] = (int)NoPlayersLifetime;

            if (LevelBoxSize is not null)
                doc["level_box_size"] = (int)LevelBoxSize;

            if (EnviropmentInfo is not null)
                doc["env_info"] = EnviropmentInfo.ToJson();

            if (XpData is not null)
                doc["xp_data"] = JsonHelpers.ParseNodeUnbuffered(XpData);

            if (Tiles is not null and { Count: > 0 })
                doc["tiles"] = JsonSerializer.SerializeToNode(Tiles);

            doc["ai_list"] = aiList;
            doc["bosses"] = bosses;

            return doc;
        }

        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is not JsonObject)
                return;

            NoPlayersLifetime = (int?)doc["no_players_lifetime"];
            ParentObjId = (int?)doc["instance_parent_obj_id"] ?? 0;
            ParentObjType = (int?)doc["instance_parent_obj_type"] ?? 0;
            ParentObjGroup = (int?)doc["instance_parent_obj_group"] ?? 0;
            ParentObjLvl = (int?)doc["instance_parent_obj_lvl"] ?? 0;
            LevelBoxSize = (int?)doc["level_box_size"];
            SpecOpsDifficulty = (int?)doc["spec_ops_difficulty"] ?? 0;
            AsteroidIntensity = (float?)doc["AsteroidIntensity"];


            if (doc["env_info"] is JsonObject enviropmentInfoNode)
                (EnviropmentInfo ??= new()).LoadFromJson(enviropmentInfoNode);

            if (doc["xp_data"] is JsonObject xpData)
                XpData = xpData.DeserializeUnbuffered<InstanceXpData>() ?? new();

            if (doc["ai_list"] is JsonArray aiListNode)
            {
                AiList = new();

                foreach (var item in aiListNode)
                {
                    if (item is JsonObject)
                    {
                        var fleet = item.DeserializeUnbuffered<InstanceAIFleet>();

                        if (fleet is not null)
                            AiList.Add(fleet);
                    }
                }
            }

            if (doc["bosses"] is JsonArray bosses)
            {
                Bosses = new();

                foreach (var item in bosses)
                {
                    if (item is JsonObject)
                    {
                        var fleet = item.DeserializeUnbuffered<InstanceMob>();

                        if (fleet is not null)
                            Bosses.Add(fleet);
                    }
                }
            }

            Tiles = doc["tiles"]?.DeserializeUnbuffered<List<TileInfo>>();
        }
    }
}

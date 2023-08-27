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
    public class InstanceExtraData : SfaObject
    {
        public int LevelBoxSize { get; set; } = 0;

        public int ParentObjId { get; set; } = 0;

        public int ParentObjType { get; set; } = 0;

        public int ParentObjGroup { get; set; } = 0;

        public int ParentObjLvl { get; set; } = 0;

        public InstanceEnviropmentInfo EnviropmentInfo { get; set; }

        public List<InstanceAIFleet> AiList { get; set; } = new();

        public List<InstanceMob> Bosses { get; set; } = new();

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

            if (LevelBoxSize > 0)
                doc["level_box_size"] = LevelBoxSize;

            if (EnviropmentInfo is not null)
                doc["env_info"] = EnviropmentInfo.ToJson();

            doc["ai_list"] = aiList;
            doc["bosses"] = bosses;

            return doc;
        }

        public override void LoadFromJson(JsonNode doc)
        {
            base.LoadFromJson(doc);

            if (doc is not JsonObject)
                return;

            ParentObjId = (int?)doc["instance_parent_obj_id"] ?? 0;
            ParentObjType = (int?)doc["instance_parent_obj_type"] ?? 0;
            ParentObjGroup = (int?)doc["instance_parent_obj_group"] ?? 0;
            ParentObjLvl = (int?)doc["instance_parent_obj_lvl"] ?? 0;

            LevelBoxSize = (int?)doc["level_box_size"] ?? 0;

            if (doc["env_info"] is JsonObject enviropmentInfoNode)
                (EnviropmentInfo ??= new()).LoadFromJson(enviropmentInfoNode);

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
        }
    }
}

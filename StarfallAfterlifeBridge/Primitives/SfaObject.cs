using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Primitives
{
    public class SfaObject : ISfaObject
    {
        public virtual void Init()
        {

        }

        public virtual JsonNode ToJson()
        {
            return new JsonObject();
        }

        public virtual void LoadFromJson(JsonNode doc)
        {

        }

        public string ToJsonString(bool writeIndented = false)
        {
            return ToJson()?.ToJsonString(writeIndented);
        }


        public virtual void LoadFromJsonString(string json)
        {
            LoadFromJson(JsonNode.Parse(json));
        }
    }
}

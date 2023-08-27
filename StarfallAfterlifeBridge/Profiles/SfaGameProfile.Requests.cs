using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Profiles
{
    public partial class SfaGameProfile
    {
        public JsonNode CreatePlayerDataRequest()
        {
            JsonNode response = new JsonObject();
            JsonArray chars = new JsonArray();

            foreach (var item in DiscoveryModeProfile.Chars)
                chars.Add(new JsonObject
                {
                    ["id"] = item.Id,
                    ["name"] = item.Name,
                });

            response["chars"] = chars;

            return response;
        }
    }
}

using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public struct RichAsteroid
    {
        [JsonPropertyName("id")]
        public int Id {  get; set; }

        [JsonPropertyName("system_id")]
        public int SystemId {  get; set; }

        [JsonPropertyName("hex")]
        public SystemHex Hex {  get; set; }
    }
}

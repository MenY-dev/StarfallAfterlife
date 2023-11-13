using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Server.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Server.Galaxy
{
    public class SecretObjectInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = -1;

        [JsonPropertyName("type")]
        public SecretObjectType Type { get; set; } = SecretObjectType.None;

        [JsonPropertyName("hex")]
        public SystemHex Hex { get; set; } = SystemHex.Zero;

        [JsonPropertyName("system")]
        public int SystemId { get; set; } = -1;

        [JsonPropertyName("lvl")]
        public int Level { get; set; } = 0;
    }
}

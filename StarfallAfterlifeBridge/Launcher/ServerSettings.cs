using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public class ServerSettings
    {
        [JsonPropertyName("realm_id")]
        public string RealmId { get; set; } = null;

        [JsonPropertyName("address")]
        public string Address { get; set; } = "0.0.0.0";

        [JsonPropertyName("port")]
        public int Port { get; set; } = 50200;

        [JsonPropertyName("use_password")]
        public bool UsePassword { get; set; } = false;

        [JsonPropertyName("password")]
        public string Password { get; set; } = null;

        [JsonPropertyName("use_port_forwarding")]
        public bool UsePortForwarding { get; set; } = true;
    }
}

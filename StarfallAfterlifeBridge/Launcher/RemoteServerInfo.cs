using StarfallAfterlife.Bridge.Serialization;
using StarfallAfterlife.Bridge.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Launcher
{
    public class RemoteServerInfo
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("need_password")]
        public bool NeedPassword { get; set; }

        [JsonPropertyName("version")]
        public Version Version { get; set; }

        [JsonIgnore]
        public bool IsOnline { get; set; }

        public Task<bool> Update(int timeout = -1, CancellationToken ct = default)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var request = SfaClient.GetServerInfo(Address, timeout);
                    request.Wait(timeout, ct);

                    if (ct.IsCancellationRequested ||
                        request.IsCompleted == false)
                    {
                        IsOnline = false;
                        return false;
                    }

                    IsOnline = true;
                    var response = request.Result;

                    if (response is JsonObject doc)
                    {
                        Version = Version.TryParse((string)doc["version"] ?? "", out var ver) ? ver : Version;
                        Id = (string)doc["realm_id"] ?? Id;
                        Name = (string)doc["realm_name"] ?? Name;
                        NeedPassword = (bool?)doc["need_password"] ?? false;
                        Description = (string)doc["realm_description"];
                    }

                    return true;
                }
                catch
                {
                    IsOnline = false;
                    return false;
                };
            });
        }
    }
}

using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Profiles;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Bridge.Instances
{
    public class InstanceManagerClientBase : MessagingClient
    {
        public IPEndPoint RemoteEndPoint => TcpClient?.Client?.RemoteEndPoint as IPEndPoint;

        protected virtual void OnReceive(string msgType, JsonNode doc) { }

        protected override void OnReceiveText(string text)
        {
            base.OnReceiveText(text);

            var doc = JsonNode.Parse(text);
            var cmd = (string)doc?["cmd"];

            if (string.IsNullOrWhiteSpace(cmd) == false)
                OnReceive(cmd, JsonNode.Parse((string)doc["msg"]));
        }

        public virtual void Send(string cmd, JsonNode doc)
        {
            base.Send(new JsonObject()
            {
                ["cmd"] = cmd,
                ["msg"] = doc?.ToJsonString()
            }.ToJsonString());
        }

        public override void Send(string text)
        {
            Send(null, text);
        }

        public override void Send(byte[] bytes)
        {
            Send(null, JsonNode.Parse(bytes));
        }
    }
}

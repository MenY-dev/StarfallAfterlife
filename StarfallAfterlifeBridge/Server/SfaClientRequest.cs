using StarfallAfterlife.Bridge.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JNode = System.Text.Json.Nodes.JsonNode;
using JObject = System.Text.Json.Nodes.JsonObject;
using JArray = System.Text.Json.Nodes.JsonArray;
using JValue = System.Text.Json.Nodes.JsonValue;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClientRequest
    {
        public SfaServerAction Action { get; }

        public string Text { get; }

        public Guid RequestId { get; }

        public SfaClientBase Client { get; }

        public SfaClientRequest(SfaClientBase client, Guid requestId, SfaServerAction action, string text)
        {
            Action = action;
            Text = text;
            RequestId = requestId;
            Client = client;
        }

        public void SendResponce(JNode responce, SfaServerAction action)
        {
            string text = responce?.ToJsonStringUnbuffered(false);
            SendResponce(text, action);
        }

        public void SendResponce(string responce, SfaServerAction action)
        {
            Client?.SendResponse(new SfaClientResponse(RequestId, responce, action));
        }
    }
}

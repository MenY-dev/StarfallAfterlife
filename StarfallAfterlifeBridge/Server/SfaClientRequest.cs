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
using StarfallAfterlife.Bridge.Networking.Messaging;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClientRequest : MessagingRequest
    {
        public SfaServerAction Action { get; init; }

        public SfaClientRequest(string text) : base(text) { }

        public SfaClientRequest(byte[] data) : base(data) { }

        public SfaClientRequest(Guid id, string text, MessagingClient client) : base(id, text, client) { }

        public SfaClientRequest(Guid id, byte[] data, MessagingClient client) : base(id, data, client) { }

        public SfaClientRequest(MessagingRequest request) : base(request)
        {
            if (request.Method == MessagingMethod.TextRequest)
            {
                var doc = JsonHelpers.ParseNodeUnbuffered(request.Text ?? "")?.AsObject();

                if (doc is not null &&
                    Enum.TryParse((string)doc["type"], true, out SfaServerAction messageAction))
                {
                    Action = messageAction;
                    Text = (string)doc["message"];
                }
            }
        }

        public void SendResponce(JNode responce, SfaServerAction action)
        {
            string text = responce?.ToJsonStringUnbuffered(false);
            SendResponce(text, action);
        }

        public void SendResponce(string responce, SfaServerAction action)
        {
            var doc = new JObject
            {
                ["type"] = JValue.Create(action),
                ["message"] = responce ?? string.Empty
            };

            Client?.SendResponse(this, doc.ToJsonStringUnbuffered(false));
        }
    }
}

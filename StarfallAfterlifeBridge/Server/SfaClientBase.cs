using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Networking.Messaging;
using StarfallAfterlife.Bridge.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JNode = System.Text.Json.Nodes.JsonNode;
using JObject = System.Text.Json.Nodes.JsonObject;
using JArray = System.Text.Json.Nodes.JsonArray;
using JValue = System.Text.Json.Nodes.JsonValue;
using StarfallAfterlife.Bridge.Serialization;
using System.Runtime;

namespace StarfallAfterlife.Bridge.Server
{
    public class SfaClientBase : MessagingClient
    {
        private Dictionary<Guid, Action<string, SfaServerAction>> RequestWaiters { get; } = new();

        private object RequestsLockher { get; } = new();

        public override void Send(string text) => Send(text, SfaServerAction.None);

        public override void Send(JsonNode node) => Send(node, SfaServerAction.None);

        public override void Send(JNode node) => Send(node, SfaServerAction.None);

        public void Send(JsonNode node, SfaServerAction messageType) =>
            Send(node?.ToJsonString(false) ?? string.Empty, messageType);

        public void Send(JNode node, SfaServerAction messageType) =>
            Send(node?.ToJsonStringUnbuffered(false) ?? string.Empty, messageType);

        public void Send(string text, SfaServerAction messageType)
        {
            var doc = new JObject
            {
                ["type"] = JValue.Create(messageType),
                ["message"] = text ?? string.Empty
            };

            base.Send(doc);
        }

        public void Send(byte[] bytes, SfaServerAction messageType)
        {
            base.Send(new byte[] { (byte)messageType }.Concat(bytes).ToArray());
        }

        public override void Send(byte[] bytes)
        {
            base.Send(new byte[] { (byte)SfaServerAction.None }.Concat(bytes).ToArray());
        }

        public Task<SfaClientResponse> SendRequest(SfaServerAction messageType, JsonNode node, int timeout = -1) =>
            SendRequest(messageType, node?.ToJsonString(false) ?? string.Empty, timeout);

        public Task<SfaClientResponse> SendRequest(SfaServerAction messageType, JNode node, int timeout = -1) =>
            SendRequest(messageType, node?.ToJsonStringUnbuffered(false) ?? string.Empty, timeout);

        public Task<SfaClientResponse> SendRequest(SfaServerAction messageType, string text = null, int timeout = -1)
        {
            var requestId = Guid.NewGuid();
            var response = new SfaClientResponse(requestId);
            var result = response.Wait(timeout);

            result.ContinueWith(t =>
            {
                lock (RequestsLockher)
                    RequestWaiters.Remove(requestId);
            });

            lock (RequestsLockher)
            {
                if (RequestWaiters.TryAdd(requestId, response.CreateHandler()))
                {
                    var doc = new JObject
                    {
                        ["type"] = JValue.Create(messageType),
                        ["request_id"] = requestId,
                        ["message"] = text ?? string.Empty
                    };

                    base.Send(doc);
                }
            }

            return result;
        }

        public void SendResponse(SfaClientResponse response)
        {
            if (response is null)
                return;

            var doc = new JObject
            {
                ["type"] = JValue.Create(response.Action),
                ["response_id"] = response.ResponseId,
                ["message"] = response.Text ?? string.Empty
            };

            base.Send(doc);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
        }

        protected override void OnReceiveText(string text)
        {
            base.OnReceiveText(text);

            try
            {
                var doc = JsonHelpers.ParseNodeUnbuffered(text)?.AsObject();

                if (doc is not null && Enum.TryParse((string)doc["type"], true, out SfaServerAction messageAction))
                {
                    string message = (string)doc["message"];

                    if (messageAction != SfaServerAction.None)
                    {
                        var responseId = (Guid?)doc["response_id"] ?? Guid.Empty;

                        if (responseId != Guid.Empty)
                        {
                            lock (RequestsLockher)
                            {
                                if (RequestWaiters.TryGetValue(responseId, out Action<string, SfaServerAction> handler) == true)
                                {
                                    RequestWaiters.Remove(responseId);
                                    handler?.Invoke(message, messageAction);
                                }
                            }
                        }
                        else
                        {
                            var requestId = (Guid?)doc["request_id"] ?? Guid.Empty;

                            if (requestId != Guid.Empty)
                                OnRequestInput(new SfaClientRequest(this, requestId, messageAction, message));
                            else
                                OnTextInput(message ?? "", messageAction);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                SfaDebug.Print(e, GetType().Name);
            }
        }



        protected override void OnReceiveBinary(SfReader reader)
        {
            base.OnReceiveBinary(reader);

            try
            {
                if (reader.Length < 0)
                    return;

                SfaServerAction binaryAction = (SfaServerAction)reader.ReadByte();

                if (binaryAction != SfaServerAction.None)
                    OnBinaryInput(reader, binaryAction);
            }
            catch (Exception e)
            {
                SfaDebug.Print(e, GetType().Name);
            }
        }

        protected virtual void OnBinaryInput(SfReader reader, SfaServerAction action)
        {

        }

        protected virtual void OnTextInput(string text, SfaServerAction action)
        {

        }

        protected virtual void OnRequestInput(SfaClientRequest request)
        {

        }
    }
}

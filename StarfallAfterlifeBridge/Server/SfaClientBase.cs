using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using StarfallAfterlife.Bridge.Networking.Channels;
using StarfallAfterlife.Bridge.Networking.Messaging;
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
        public override void Send(string text) => Send(text, SfaServerAction.None);

        public override void Send(JNode node) => Send(node, SfaServerAction.None);

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

        public Task<SfaClientResponse> SendRequest(SfaServerAction messageType, JNode node, int timeout = -1) =>
            SendRequest(messageType, node?.ToJsonStringUnbuffered(false) ?? string.Empty, timeout);

        public Task<SfaClientResponse> SendRequest(SfaServerAction messageType, string text = null, int timeout = -1)
        {
            var doc = new JObject
            {
                ["type"] = JValue.Create(messageType),
                ["message"] = text ?? string.Empty
            };

            return SendRequestInternal
                (doc.ToJsonStringUnbuffered(true),
                (id, method) => new SfaClientResponse { Id = id, Method = method, Action = messageType })
                .Wait(timeout)
                .ContinueWith(t => t.Result as SfaClientResponse);
        }

        public Task<SfaClientResponse> SendRequest(SfaServerAction messageType, ReadOnlyMemory<byte> data, int timeout = -1)
        {
            var packet = new byte[data.Length + 1];
            packet[0] = (byte)messageType;

            return SendRequestInternal(
                packet,
                (id, method) => new SfaClientResponse { Id = id, Method = method, Action = messageType })
                .Wait(timeout)
                .ContinueWith(t => t.Result as SfaClientResponse);
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
                    OnTextInput(message ?? "", messageAction);
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

        protected override void OnReceiveRequest(MessagingRequest request)
        {
            base.OnReceiveRequest(request);

            try
            {
                OnRequestInput(new SfaClientRequest(request));
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

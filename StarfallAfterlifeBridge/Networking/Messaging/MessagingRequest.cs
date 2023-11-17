using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingRequest
    {
        public Guid Id { get; init; }

        public MessagingMethod Method { get; init; }

        public string Text { get; init; }

        public ReadOnlyMemory<byte> Data { get; init; }

        public MessagingClient Client { get; init; }

        public MessagingRequest(string text)
        {
            Method = MessagingMethod.TextRequest;
            Text = text;
        }

        public MessagingRequest(byte[] data)
        {
            Method = MessagingMethod.BinaryRequest;
            Data = data;
        }

        public MessagingRequest(Guid id, string text, MessagingClient client)
        {
            Id = id;
            Method = MessagingMethod.TextRequest;
            Text = text;
            Client = client;
        }

        public MessagingRequest(Guid id, byte[] data, MessagingClient client)
        {
            Id = id;
            Method = MessagingMethod.BinaryRequest;
            Data = data;
            Client = client;
        }

        public MessagingRequest(MessagingRequest request)
        {
            Id = request.Id;
            Method = request.Method;
            Text = request.Text;
            Data = request.Data;
            Client = request.Client;
        }

        public virtual ReadOnlyMemory<byte> CreateBinaryMessage() => Data;

        public virtual string CreateTextMessage() => Text;

        public virtual void SendResponce(string text)
        {
            Client?.SendResponse(this, text);
        }

        public virtual void SendResponce(byte[] data)
        {
            Client?.SendResponse(this, data);
        }
    }
}

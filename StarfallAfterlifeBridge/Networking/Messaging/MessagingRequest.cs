using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public class MessagingRequest
    {
        public Guid Id { get; }

        public MessagingMethod Method { get; }

        public string Text { get; }

        public byte[] Data { get; }

        public MessagingClient Client { get; }

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

        public virtual void SendResponce(string text)
        {
            Client?.SendResponse(MessagingResponse.Create(Id, text));
        }

        public virtual void SendResponce(byte[] data)
        {
            Client?.SendResponse(MessagingResponse.Create(Id, data));
        }
    }
}

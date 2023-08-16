using StarfallAfterlife.Bridge.Diagnostics;
using StarfallAfterlife.Bridge.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Channels
{
    public class Channel
    {
        public string Name { get; } = string.Empty;

        public int Id { get; } = -1;

        public event EventHandler<TextInputEventArgs> TextInput;

        public event EventHandler<BinaryInputEventArgs> BinaryInput;

        public Channel(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public virtual void Register(ChannelClient client)
        {
            client?.Use((reader, writer) =>
            {
                SfaDebug.Print($"Register (Id = {Id}, Name = {Name})", "Channel");

                writer.WriteSfcp(new SFCP.RegisterResponse()
                {
                    ErrorCode = 0,
                    ChannelId = Id,
                    ChannelName = Name
                });
            });
        }

        public virtual void Input(ChannelClient client, string text)
        {
            SfaDebug.Print($"TextInput (Id = {Id}, Name = {Name}, Text = {text})", GetType().Name);
            OnTextInput(new(client, text));
        }

        public virtual void Input(ChannelClient client, byte[] data)
        {
            SfaDebug.Print($"BinaryInput (Id = {Id}, Name = {Name}, Data = {BitConverter.ToString(data).Replace("-", "")}))", GetType().Name);
            OnBinaryInput(new(client, data));
        }

        public virtual void Send(ChannelClient client, string text, int ErrorCode = 0, Encoding encoding = null)
        {
            client?.Send(Id, text, ErrorCode, encoding);
        }

        public virtual void Send(ChannelClient client, byte[] data, int ErrorCode = 0)
        {
            client?.Send(Id, data, ErrorCode);
        }

        protected virtual void OnTextInput(TextInputEventArgs args) => TextInput?.Invoke(this, args);

        protected virtual void OnBinaryInput(BinaryInputEventArgs args) => BinaryInput?.Invoke(this, args);
    }
}
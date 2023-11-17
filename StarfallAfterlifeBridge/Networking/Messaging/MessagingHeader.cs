using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.Networking.Messaging
{
    public struct MessagingHeader
    {
        public MessagingMethod Method { get; set; }

        public int Length { get; set; }

        public Guid RequestId { get; set; }

        private static readonly byte[] BinaryMethod = Encoding.ASCII.GetBytes("BYTE");
        private static readonly byte[] BinaryRequestMethod = Encoding.ASCII.GetBytes("BYTER");
        private static readonly byte[] TextMethod = Encoding.ASCII.GetBytes("TEXT");
        private static readonly byte[] TextRequestMethod = Encoding.ASCII.GetBytes("TEXTR");
        private static readonly byte[] HttpMethod = Encoding.ASCII.GetBytes("GET");
        private static readonly byte MethodEndCode = Encoding.ASCII.GetBytes(" ")[0];

        public void Write(Stream stream)
        {
            switch (Method)
            {
                case MessagingMethod.Binary:
                    stream.Write(BinaryMethod);
                    stream.WriteByte(MethodEndCode);
                    stream.Write(BitConverter.GetBytes(Length), 0, 4);
                    break;
                case MessagingMethod.TextRequest:
                    stream.Write(TextRequestMethod);
                    stream.WriteByte(MethodEndCode);
                    stream.Write(RequestId.ToByteArray(), 0, 16);
                    stream.Write(BitConverter.GetBytes(Length), 0, 4);
                    break;
                case MessagingMethod.Text:
                    stream.Write(TextMethod);
                    stream.WriteByte(MethodEndCode);
                    stream.Write(BitConverter.GetBytes(Length), 0, 4);
                    break;
                case MessagingMethod.BinaryRequest:
                    stream.Write(BinaryRequestMethod);
                    stream.WriteByte(MethodEndCode);
                    stream.Write(RequestId.ToByteArray(), 0, 16);
                    stream.Write(BitConverter.GetBytes(Length), 0, 4);
                    break;
                case MessagingMethod.HTTP:
                    stream.Write(HttpMethod);
                    stream.WriteByte(MethodEndCode);
                    break;
            }
        }

        public static MessagingHeader ReadNext(Stream stream, CancellationToken ct = default)
        {
            var header = new MessagingHeader();
            header.Method = ReadNextMethod(stream, ct);

            ct.ThrowIfCancellationRequested();

            if (header.Method == MessagingMethod.Binary ||
                header.Method == MessagingMethod.Text)
            {
                var buffer = new byte[4];
                stream.Read(buffer, 0, buffer.Length);
                header.Length = BitConverter.ToInt32(buffer);
            }
            else if (header.Method == MessagingMethod.BinaryRequest ||
                header.Method == MessagingMethod.TextRequest)
            {
                var buffer = new byte[20];
                stream.Read(buffer, 0, buffer.Length);
                header.RequestId = new Guid(buffer);
                header.Length = BitConverter.ToInt32(buffer, 16);
            }

            ct.ThrowIfCancellationRequested();
            return header;
        }

        private static MessagingMethod ReadNextMethod(Stream stream, CancellationToken ct = default)
        {
            using var buffer = new MemoryStream(32);
            var type = MessagingMethod.Unknown;

            while (ct.IsCancellationRequested == false)
            {
                ReadWhile(stream, buffer, MethodEndCode, ct);

                var methodEndLocation = (int)buffer.Position - 1;

                if (methodEndLocation > 0)
                    type = ReadMethod(new ReadOnlySpan<byte>(buffer.GetBuffer(), 0, methodEndLocation));

                if (type != MessagingMethod.Unknown)
                    return type;

                buffer.Seek(0, SeekOrigin.Begin);
            }

            ct.ThrowIfCancellationRequested();
            return type;
        }

        private static void ReadWhile(Stream stream, Stream buffer, byte stopBite, CancellationToken ct = default)
        {
            byte[] chunk = new byte[1];

            while (ct.IsCancellationRequested == false)
            {
                if (stream.Read(chunk) < 1)
                    throw new IOException();

                buffer.WriteByte(chunk[0]);

                if (chunk[0] == stopBite)
                    return;
            }

            ct.ThrowIfCancellationRequested();
        }

        private static MessagingMethod ReadMethod(ReadOnlySpan<byte> bytes)
        {
            if (CheckMethod(BinaryMethod, bytes) == true) return MessagingMethod.Binary;
            if (CheckMethod(TextRequestMethod, bytes) == true) return MessagingMethod.TextRequest;
            if (CheckMethod(TextMethod, bytes) == true) return MessagingMethod.Text;
            if (CheckMethod(BinaryRequestMethod, bytes) == true) return MessagingMethod.BinaryRequest;
            if (CheckMethod(HttpMethod, bytes) == true) return MessagingMethod.HTTP;
            return MessagingMethod.Unknown;
        }

        private static bool CheckMethod(byte[] method, ReadOnlySpan<byte> bytes)
        {
            if (method.Length > bytes.Length)
                return false;

            int startPos = bytes.Length - method.Length;

            for (int i = 0; i < method.Length; i++)
            {
                if (method[i] != bytes[startPos + i])
                    return false;
            }

            return true;
        }
    }
}

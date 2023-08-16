using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StarfallAfterlife.Bridge.Mathematics;
using StarfallAfterlife.Bridge.Networking;

namespace StarfallAfterlife.Bridge.IO
{
    public partial class SfWriter : IDisposable
    {
        public Stream Stream { get; protected set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        protected BinaryWriter Writer { get; set; }

        protected bool CreatedWithInternalStream { get; }

        public SfWriter() : this(new MemoryStream())
        {
            CreatedWithInternalStream = true;
        }

        public SfWriter(int capacity) : this(new MemoryStream(capacity))
        {
            CreatedWithInternalStream = true;
        }

        public SfWriter(byte[] buffer) : this(new MemoryStream(buffer))
        {
            CreatedWithInternalStream = true;
        }

        public SfWriter(Stream stream)
        {
            Stream = stream;
            Writer = new BinaryWriter(stream, Encoding, true);
        }

        public byte[] ToArray()
        {
            if (Stream is MemoryStream memoryStream)
                return memoryStream.ToArray();

            return Array.Empty<byte>();
        }

        public void Write(SFCP.ISFCP packet)
        {
            int size = Marshal.SizeOf(packet);
            Write(SFCP.BuildHeader(packet, (ushort)size));
            Write(packet);
        }

        public void Write<T>(T data)
        {
            int size = Marshal.SizeOf(data);
            byte[] bytes = new byte[size];
            IntPtr buffer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(data, buffer, false);
            Marshal.Copy(buffer, bytes, 0, size);
            Marshal.FreeHGlobal(buffer);

            Write(bytes, 0, size);
        }

        public virtual void WriteString(string text, int size = -1, bool writeTextSize = false, Encoding encoding = null)
        {
            if (encoding is null)
                encoding = Encoding ?? Encoding.ASCII;

            byte[] buffer = encoding.GetBytes(text);

            if (writeTextSize == true)
                Stream.Write(BitConverter.GetBytes((uint)buffer.Length), 0, 4);

            if (size < 0)
            {
                Stream.Write(buffer, 0, Math.Min(buffer.Length, int.MaxValue));
            }
            else
            {
                Stream.Write(buffer, 0, Math.Min(buffer.Length, size));

                if (buffer.Length < size)
                    for (int i = buffer.Length; i < size; i++)
                        Stream.WriteByte(0);
            }
        }

        public virtual void WriteShortString(string text, short size = -1, bool writeTextSize = false, Encoding encoding = null)
        {
            if (encoding is null)
                encoding = Encoding ?? Encoding.ASCII;

            byte[] buffer = encoding.GetBytes(text);

            if (writeTextSize == true)
                Stream.Write(BitConverter.GetBytes((ushort)buffer.Length), 0, 2);

            if (size < 0)
            {
                Stream.Write(buffer, 0, Math.Min(buffer.Length, ushort.MaxValue));
            }
            else
            {
                Stream.Write(buffer, 0, Math.Min(buffer.Length, size));

                if (buffer.Length < size)
                    for (int i = buffer.Length; i < size; i++)
                        Stream.WriteByte(0);
            }
        }

        public virtual void WriteBoolean(bool value) => Writer.Write(value);

        public virtual void WriteByte(byte value) => Writer.Write(value);

        public virtual void WriteInt16(short value) => Writer.Write(value);

        public virtual void WriteInt32(int value) => Writer.Write(value);

        public virtual void WriteInt64(long value) => Writer.Write(value);

        public virtual void WriteUInt16(ushort value) => Writer.Write(value);

        public virtual void WriteUInt32(uint value) => Writer.Write(value);

        public virtual void WriteUInt64(ulong value) => Writer.Write(value);

        public virtual void WriteSingle(float value) => Writer.Write(value);

        public virtual void WriteDouble(double value) => Writer.Write(value);

        public virtual void WriteDecimal(decimal value) => Writer.Write(value);

        public virtual void WriteChar(char value) => Writer.Write(value);

        public virtual void WriteVector2(Vector2 value)
        {
            WriteSingle(value.X);
            WriteSingle(value.Y);
        }

        public virtual void WriteHex(SystemHex value)
        {
            WriteInt32(value.X);
            WriteInt32(value.Y);
        }

        public virtual void Write(byte[] buffer) => Writer.Write(buffer);

        public virtual void Write(byte[] buffer, int index, int count) => Writer.Write(buffer, index, count);

        public void Dispose()
        {
            if (CreatedWithInternalStream == true)
                Stream.Dispose();

            Stream = null;
            Writer.Dispose();
        }
    }
}

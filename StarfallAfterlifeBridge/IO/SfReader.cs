using StarfallAfterlife.Bridge.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.IO
{
    public partial class SfReader : IDisposable
    {
        public Stream Stream { get; protected set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public long Length
        {
            get
            {
                try
                {
                    return Stream.Length;
                }
                catch { }

                return 0;
            }
        }


        public long Position
        {
            get
            {
                try
                {
                    return Stream.Position;
                }
                catch { }

                return 0;
            }
        }

        protected BinaryReader Reader { get; set; }

        protected bool CreatedFromBytes { get; }

        public SfReader(Stream stream)
        {
            Stream = stream;
            Reader = new BinaryReader(stream, Encoding, true);
        }

        public SfReader(byte[] buffer) : this(new MemoryStream(buffer))
        {
            CreatedFromBytes = true;
        }

        private async Task<int> ReadPacketAsync(byte[] buffer, int size, CancellationToken cancellation = new CancellationToken())
        {
            if (Stream is null || buffer.Length < size)
                return -1;

            int position = 0;

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                while (position < size && cancellation.IsCancellationRequested == false)
                {
                    byte[] chunk = new byte[size];
                    int bytesCount = await Stream.ReadAsync(chunk.AsMemory(0, size - position), cancellation);

                    if (bytesCount > 0)
                    {
                        position += bytesCount;
                        ms.Write(chunk, 0, bytesCount);
                    }
                }
            }

            return position;
        }

        private int ReadPacket(byte[] buffer, int size, CancellationToken cancellation = new CancellationToken())
        {
            return ReadPacketAsync(buffer, size, cancellation).Result;
        }

        public virtual byte[] ReadToEnd()
        {
            long length = Length - Position;
            byte[] buffer = new byte[length < 1 ? 0 : length];
            Read(buffer);
            return buffer;
        }

        public virtual string ReadString(Encoding encoding = null)
        {
            uint size = ReadUInt32();
            return ReadString((int)size, encoding);
        }

        public virtual string ReadShortString(Encoding encoding = null)
        {
            int size = ReadUInt16();
            return ReadString(size, encoding);
        }

        public virtual string ReadString(int size, Encoding encoding = null)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, null);

            if (size == 0)
                return string.Empty;

            if (encoding is null)
                encoding = Encoding ?? Encoding.ASCII;

            byte[] buffer = new byte[size];
            Read(buffer, 0, size);
            return encoding.GetString(buffer)?.TrimEnd('\0');
        }

        public virtual bool ReadBoolean() => Reader.ReadBoolean();

        public virtual byte ReadByte() => Reader.ReadByte();

        public virtual short ReadInt16() => Reader.ReadInt16();

        public virtual int ReadInt32() => Reader.ReadInt32();

        public virtual long ReadInt64() => Reader.ReadInt64();

        public virtual ushort ReadUInt16() => Reader.ReadUInt16();

        public virtual uint ReadUInt32() => Reader.ReadUInt32();

        public virtual ulong ReadUInt64() => Reader.ReadUInt64();

        public virtual float ReadSingle() => Reader.ReadSingle();

        public virtual double ReadDouble() => Reader.ReadDouble();

        public virtual decimal ReadDecimal() => Reader.ReadDecimal();

        public virtual char ReadChar() => Reader.ReadChar();

        public virtual Vector2 ReadVector2() => new Vector2(Reader.ReadSingle(), Reader.ReadSingle());

        public virtual SystemHex ReadHex() => new SystemHex(Reader.ReadInt32(), Reader.ReadInt32());

        public virtual int Read() => Reader.Read();

        public virtual int Read(byte[] buffer) => buffer is null ? 0 : Reader.Read(buffer, 0, buffer.Length);

        public virtual int Read(byte[] buffer, int index, int count) => Reader.Read(buffer, index, count);

        public void Dispose()
        {
            if (CreatedFromBytes == true)
                Stream.Dispose();

            Stream = null;
            Reader.Dispose();
        }
    }
}

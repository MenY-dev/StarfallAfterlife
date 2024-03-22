using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.SfPackageLoader
{
    public class FArchive
    {
        public Stream Stream { get; protected set; }

        public long Position => Stream.Position;

        public long Length => Stream.Length;

        public int BufferSize => _buffer?.BufferSize ?? 0;

        private BinaryReader _reader;

        private byte[] _textBuffer;

        private Stream _baseStream;

        private BufferedStream _buffer;

        private bool _isInternalStream;

        public FArchive(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            _isInternalStream = true;
            _baseStream = File.OpenRead(path);
            SetBufferSize(0);
        }

        public FArchive(Stream baseStream)
        {
            if (baseStream is null)
                throw new ArgumentNullException(nameof(baseStream));

            _isInternalStream = false;
            _baseStream = baseStream;
            SetBufferSize(0);
        }

        public static FArchive Open(string path) => new FArchive(path);

        public void Seek(long offset, SeekOrigin origin = SeekOrigin.Current) => Stream.Seek(offset, origin);

        public void Skip(long count) => Stream.Seek(count, SeekOrigin.Current);

        public bool ReadBoolean() => _reader.ReadBoolean();

        public byte ReadByte() => _reader.ReadByte();

        public short ReadInt8() => _reader.ReadSByte();

        public short ReadInt16() => _reader.ReadInt16();

        public int ReadInt32() => _reader.ReadInt32();

        public long ReadInt64() => _reader.ReadInt64();

        public ushort ReadUInt16() => _reader.ReadUInt16();

        public uint ReadUInt32() => _reader.ReadUInt32();

        public ulong ReadUInt64() => _reader.ReadUInt64();

        public float ReadSingle() => _reader.ReadSingle();

        public double ReadDouble() => _reader.ReadDouble();

        public decimal ReadDecimal() => _reader.ReadDecimal();

        public char ReadChar() => _reader.ReadChar();

        public byte[] ReadBytes(int count) => _reader.ReadBytes(count);

        public T[] ReadArray<T>() where T : IFArchiveSerializable, new()
        {
            int count = Math.Max(0, _reader.ReadInt32());
            T[] array = new T[count];

            for (int i = 0; i < count; i++)
            {
                T item = new();
                item.Deserialize(this);
                array[i] = item;
            }

            return array;
        }

        public T[] ReadArray<T>(Func<FArchive, T> reader)
        {
            int count = Math.Max(0, _reader.ReadInt32());
            T[] array = new T[count];

            for (int i = 0; i < count; i++)
                array[i] = reader.Invoke(this);

            return array;
        }

        public T Read<T>() where T : IFArchiveSerializable, new()
        {
            T val = new();
            val.Deserialize(this);
            return val;
        }

        public int Read() => _reader.Read();

        public int Read(byte[] buffer) => buffer is null ? 0 : _reader.Read(buffer, 0, buffer.Length);

        public int Read(byte[] buffer, int index, int count) => _reader.Read(buffer, index, count);

        public void Read<T>(T self) where T : class, IFArchiveSerializable => self.Deserialize(this);

        public void Read<T>(ref T self) where T : struct, IFArchiveSerializable => self.Deserialize(this);

        public FName ReadName()
        {
            var value = new FName();
            Read(ref value);
            return value;
        }

        public (FName Name, int Class) ReadSoftClassPtr() => (ReadName(), ReadInt32());

        public string ReadStrig(int bytesCount, Encoding encoding = default)
        {
            if (bytesCount == 0)
                return string.Empty;

            if (bytesCount < 0 || bytesCount > Array.MaxLength)
                throw new ArgumentOutOfRangeException(nameof(bytesCount));

            if (_textBuffer is null || _textBuffer.Length < bytesCount)
                _textBuffer = new byte[Math.Min(Array.MaxLength, (int)(bytesCount * 1.5f))];

            encoding ??= Encoding.ASCII;

            Stream.ReadExactly(_textBuffer, 0, bytesCount);
            bytesCount = Math.Max(0, bytesCount - (encoding.IsSingleByte ? 1 : 2));
            return encoding.GetString(_textBuffer, 0, bytesCount);
        }

        public string ReadString()
        {
            var encoding = Encoding.ASCII;
            var count = ReadInt32();

            if (count < 0)
            {
                count *= -2;
                encoding = Encoding.Unicode;
            }

            return ReadStrig(count, encoding);
        }

        public void SetBufferSize(int size)
        {
            if (size > 0)
            {
                _buffer = new BufferedStream(_baseStream, size);
                Stream = _buffer;
            }
            else
            {
                _buffer = null;
                Stream = _baseStream;
            }

            CreateReader(Stream);
        }

        protected void CreateReader(Stream stream)
        {
            _reader?.Close();
            _reader = new BinaryReader(stream, Encoding.ASCII, true);
        }

        public virtual void Dispose()
        {
            if (_isInternalStream == true)
            {
                _baseStream?.Dispose();
            }

            _reader.Close();
            _reader = null;
            _buffer = null;
            _textBuffer = null;
            _baseStream = null;
        }
    }
}

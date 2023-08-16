using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.IO
{
    public class PooledStream : Stream, IBufferWriter<byte>
    {
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > int.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value > _length)
                    SetLength(value);

                _position = (int)value;
            }
        }

        public override void Flush() { }

        public int Capacity
        {
            get
            {
                return _buffer.Length;
            }
        }

        public Memory<byte> Memory => _buffer.AsMemory(0, _length);

        public Span<byte> Span => _buffer.AsSpan(0, _length);

        public const int DefaultCapacity = 256;

        private byte[] _buffer;
        private int _length;
        private int _position;


        public PooledStream(int initialCapacity = 0)
        {
            RentBuffer(Math.Max(initialCapacity, DefaultCapacity));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesToRead = _length - _position;

            if (bytesToRead > count)
                bytesToRead = count;

            if (bytesToRead <= 0)
                return 0;

            if (bytesToRead <= 8)
            {
                int byteCount = bytesToRead;

                while (--byteCount >= 0)
                    buffer[offset + byteCount] = _buffer[_position + byteCount];
            }
            else
                Buffer.BlockCopy(_buffer, _position, buffer, offset, bytesToRead);

            _position += bytesToRead;

            return bytesToRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(offset));

            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        if (offset < 0)
                            throw new IOException();

                        _position = (int)offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        long newPosition = unchecked(_position + offset);

                        if (newPosition < 0 || newPosition > int.MaxValue)
                            throw new IOException();

                        _position = (int)newPosition;
                        break;
                    }
                case SeekOrigin.End:
                    {
                        long newPosition = unchecked(_length + (int)offset);

                        if (newPosition < 0 || newPosition > int.MaxValue)
                            throw new IOException();

                        _position = (int)newPosition;
                        break;
                    }
            }

            if (_position > _length)
                SetLength(_position);

            return _position;
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            int newPos = _position + count;

            if (newPos < 0)
                throw new IOException();

            if (newPos > _length)
                SetLength(newPos);

            if (count <= 8)
            {
                int byteCount = count;

                while (--byteCount >= 0)
                    _buffer[_position + byteCount] = buffer[offset + byteCount];
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, _buffer, _position, count);
            }

            _position = newPos;
        }

        public override void SetLength(long value)
        {
            if (value < 0 || value > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value));

            int newLength = Math.Max(0, (int)value);


            if (_buffer is null || _buffer.Length < newLength)
            {
                RentBuffer(newLength + newLength / 4);
            }

            _length = newLength;

            if (_position > newLength)
                _position = newLength;
        }

        public void TrimBufferToLength()
        {
            RentBuffer(Math.Max(0, _length));
        }

        protected virtual void RentBuffer(int capacity)
        {
            var newBuffer = ArrayPool<byte>.Shared.Rent(capacity);

            if (_buffer is not null && capacity > 0 && _length > 0)
            {
                _buffer.AsSpan(0, _length).CopyTo(newBuffer);
            }

            ReturnBuffer();
            _buffer = newBuffer;
        }

        protected virtual void ReturnBuffer()
        {
            if (_buffer is null)
                return;

            byte[] currentBuffer = _buffer;
            _buffer = null;
            ArrayPool<byte>.Shared.Return(currentBuffer);
        }

        protected void ExpandBuffer(int additionalSize)
        {
            if (additionalSize < 0 || (_buffer.LongLength + additionalSize) > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(additionalSize));

            if (_buffer is null)
            {
                RentBuffer(additionalSize);
            }
            else
            {
                RentBuffer(_buffer.Length + additionalSize);
            }
        }

        void IBufferWriter<byte>.Advance(int count)
        {
            _position += count;
            _length = Math.Max(_position, _length);
        }

        Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
        {
            ExpandBuffer(sizeHint * 2);
            return _buffer.AsMemory(_position);
        }

        Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
        {
            ExpandBuffer(sizeHint * 2);
            return _buffer.AsSpan(_position);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            ReturnBuffer();
        }
    }
}

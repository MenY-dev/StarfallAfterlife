using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarfallAfterlife.Bridge.IO
{
    public class ReadOnlyMemoryStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _memory.Length;

        public override long Position { get => _pos; set => throw new NotImplementedException(); }

        private readonly ReadOnlyMemory<byte> _memory;
        private int _pos;

        public ReadOnlyMemoryStream(ReadOnlyMemory<byte> memory)
        {
            _memory = memory;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int newPos = Math.Min(_memory.Length, _pos + count);
            int delta = newPos - _pos;
            _memory.Span.Slice(_pos, delta).CopyTo(new(buffer, offset, delta));
            _pos = newPos;
            return delta;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

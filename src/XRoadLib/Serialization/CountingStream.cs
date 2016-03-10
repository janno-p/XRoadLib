using System.IO;

namespace XRoadLib.Serialization
{
    internal class CountingStream : Stream
    {
        private long writeCount = 0;
        private readonly Stream stream;

        public long WriteCount => writeCount;

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;

        public override long Position { get { return stream.Position; } set { stream.Position = value; } }

        public CountingStream(Stream stream)
        {
            this.stream = stream;
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writeCount += count - offset;
            stream.Write(buffer, offset, count);
        }
    }
}
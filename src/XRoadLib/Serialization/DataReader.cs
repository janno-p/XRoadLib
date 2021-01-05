using System;
using System.IO;
using System.Threading.Tasks;

namespace XRoadLib.Serialization
{
    public class DataReader : IDisposable
    {
        internal const int BufferSize = 4_096;

        private Stream _stream;

        private readonly byte[] _buffer = new byte[BufferSize];
        private uint _bufferPosition;
        private int _bufferSize = -1;

        public long Position { get; private set; }

        public DataReader(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
            _stream = null;
        }

        public bool Reset()
        {
            if (_stream.CanSeek)
            {
                if (_stream.Length == 0)
                    return false;

                _stream.Seek(0, SeekOrigin.Begin);
            }

            _bufferPosition = 0;
            _bufferSize = -1;

            Position = 0;

            return true;
        }

        public async Task<int> PeekByteAsync()
        {
            if (_bufferSize < 0)
                await UpdateBufferAsync();

            if (_bufferSize == 0)
                return -1;

            return _buffer[_bufferPosition];
        }

        public async Task<int> ReadByteAsync()
        {
            if (_bufferSize < 0)
                await UpdateBufferAsync();

            if (_bufferSize == 0)
                return -1;

            var byt = _buffer[_bufferPosition++];
            Position++;

            if (_bufferPosition >= _bufferSize)
                await UpdateBufferAsync();

            return byt;
        }

        private async Task UpdateBufferAsync()
        {
            _bufferSize = await _stream.ReadAsync(_buffer, 0, BufferSize);
            _bufferPosition = 0;
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;

namespace XRoadLib.Serialization
{
    internal class StreamCounter : IDisposable
    {
        private Stream _stream;

        public long WriteCount { get; private set; }

        public StreamCounter(Stream stream)
        {
            _stream = stream;
        }

        public Task FlushAsync() =>
            _stream.FlushAsync();

        public async Task WriteAsync(byte[] buffer, int offset, int count)
        {
            WriteCount += count - offset;
            await _stream.WriteAsync(buffer, offset, count).ConfigureAwait(false);
        }

        public async Task WriteAsync(Stream stream)
        {
            const int bufferSize = 4_096;

            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, bufferSize)) > 0)
                await WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
        }

        public void Dispose()
        {
            _stream.Dispose();
            _stream = null;
        }
    }
}
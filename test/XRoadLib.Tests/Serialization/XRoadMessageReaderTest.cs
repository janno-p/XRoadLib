using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XRoadLib.Serialization;
using XRoadLib.Soap;
using Xunit;
using static XRoadLib.Serialization.XRoadMessageReader;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadMessageReaderTest
    {
        private static readonly IMessageFormatter MessageFormatter = new SoapMessageFormatter();

        [Fact]
        public async Task CanHandleBufferLimit()
        {
            using var stream = new MemoryStream(Enumerable.Repeat((byte)32, 10).ToArray());
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>());

            stream.Position = 0;

            var (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(3);
            Assert.Equal(ChunkStop.BufferLimit, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(3);
            Assert.Equal(ChunkStop.BufferLimit, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(3);
            Assert.Equal(ChunkStop.BufferLimit, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(3);
            Assert.Equal(ChunkStop.EndOfStream, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x));
        }

        [Fact]
        public async Task CanHandleLineMarker()
        {
            using var stream = new MemoryStream(new byte[] { 32, 32, 32, 32, 13, 10, 32, 32, 32, 32 });
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>());

            stream.Position = 0;

            var (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(10);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(10);
            Assert.Equal(ChunkStop.EndOfStream, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));
        }

        [Fact]
        public async Task CanHandleChunkBeginningWithMarker()
        {
            using var stream = new MemoryStream(new byte[] { 32, 32, 32, 32, 13, 10, 32, 32, 32, 32 });
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>());

            stream.Position = 0;

            var (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(4);
            Assert.Equal(ChunkStop.BufferLimit, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(4);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Empty(chunk);

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(4);
            Assert.Equal(ChunkStop.BufferLimit, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(4);
            Assert.Equal(ChunkStop.EndOfStream, chunkStop);
            Assert.Empty(chunk);
        }

        [Fact]
        public async Task CanHandleSplittingMarker()
        {
            using var stream = new MemoryStream(new byte[] { 32, 32, 32, 32, 13, 10, 32, 32, 32, 32 });
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>());

            stream.Position = 0;

            var (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.EndOfStream, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));
        }

        [Fact]
        public async Task CanHandleMultipleMarkersInARow()
        {
            using var stream = new MemoryStream(new byte[] { 40, 13, 10, 13, 10, 13, 10, 13, 10, 40 });
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>());

            stream.Position = 0;

            var (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(40, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Empty(chunk);

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Empty(chunk);

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Empty(chunk);

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.EndOfStream, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(40, x));
        }

        [Fact]
        public async Task CanHandleRecurringMarkerBufferLimit()
        {
            using var stream = new MemoryStream(new byte[] { 40, 13, 13, 13, 13, 13, 13, 10, 33, 34, 40, 40 });
            using var reader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>());

            stream.Position = 0;

            var (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.BufferLimit, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(40, x), x => Assert.Equal(13, x), x => Assert.Equal(13, x), x => Assert.Equal(13, x), x => Assert.Equal(13, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.NewLine, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(13, x));

            (chunkStop, chunk) = await reader.ReadChunkOrLineAsync(5);
            Assert.Equal(ChunkStop.EndOfStream, chunkStop);
            Assert.Collection(chunk, x => Assert.Equal(33, x), x => Assert.Equal(34, x), x => Assert.Equal(40, x), x => Assert.Equal(40, x));
        }
    }
}
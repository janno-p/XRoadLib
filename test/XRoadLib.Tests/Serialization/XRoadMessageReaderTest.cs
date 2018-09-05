using System.IO;
using System.Linq;
using XRoadLib.Serialization;
using XRoadLib.Soap;
using Xunit;
using static XRoadLib.Serialization.XRoadMessageReader;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadMessageReaderTest
    {
        private static readonly IMessageFormatter messageFormatter = new SoapMessageFormatter();

        [Fact]
        public void CanHandleBufferLimit()
        {
            using (var stream = new MemoryStream(Enumerable.Repeat((byte)32, 10).ToArray()))
            using (var reader = new XRoadMessageReader(stream, messageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>()))
            {
                stream.Position = 0;

                Assert.Equal(ChunkStop.BufferLimit, reader.ReadChunkOrLine(out var chunk, 3));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.BufferLimit, reader.ReadChunkOrLine(out chunk, 3));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.BufferLimit, reader.ReadChunkOrLine(out chunk, 3));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.EndOfStream, reader.ReadChunkOrLine(out chunk, 3));
                Assert.Collection(chunk, x => Assert.Equal(32, x));
            }
        }

        [Fact]
        public void CanHandleLineMarker()
        {
            using (var stream = new MemoryStream(new byte[] { 32, 32, 32, 32, 13, 10, 32, 32, 32, 32 }))
            using (var reader = new XRoadMessageReader(stream, messageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>()))
            {
                stream.Position = 0;

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out var chunk, 10));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.EndOfStream, reader.ReadChunkOrLine(out chunk, 10));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));
            }
        }

        [Fact]
        public void CanHandleChunkBeginningWithMarker()
        {
            using (var stream = new MemoryStream(new byte[] { 32, 32, 32, 32, 13, 10, 32, 32, 32, 32 }))
            using (var reader = new XRoadMessageReader(stream, messageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>()))
            {
                stream.Position = 0;

                byte[] chunk;

                Assert.Equal(ChunkStop.BufferLimit, reader.ReadChunkOrLine(out chunk, 4));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out chunk, 4));
                Assert.Empty(chunk);

                Assert.Equal(ChunkStop.BufferLimit, reader.ReadChunkOrLine(out chunk, 4));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.EndOfStream, reader.ReadChunkOrLine(out chunk, 4));
                Assert.Empty(chunk);
            }
        }

        [Fact]
        public void CanHandleSplittingMarker()
        {
            using (var stream = new MemoryStream(new byte[] { 32, 32, 32, 32, 13, 10, 32, 32, 32, 32 }))
            using (var reader = new XRoadMessageReader(stream, messageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>()))
            {
                stream.Position = 0;

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out var chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));

                Assert.Equal(ChunkStop.EndOfStream, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x), x => Assert.Equal(32, x));
            }
        }

        [Fact]
        public void CanHandleMultipleMarkersInARow()
        {
            using (var stream = new MemoryStream(new byte[] { 40, 13, 10, 13, 10, 13, 10, 13, 10, 40 }))
            using (var reader = new XRoadMessageReader(stream, messageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>()))
            {
                stream.Position = 0;

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out var chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(40, x));

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Empty(chunk);

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Empty(chunk);

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Empty(chunk);

                Assert.Equal(ChunkStop.EndOfStream, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(40, x));
            }
        }

        [Fact]
        public void CanHandleRecurringMarkerBufferLimit()
        {
            using (var stream = new MemoryStream(new byte[] { 40, 13, 13, 13, 13, 13, 13, 10, 33, 34, 40, 40 }))
            using (var reader = new XRoadMessageReader(stream, messageFormatter, "text/xml; charset=UTF-8", Path.GetTempPath(), Enumerable.Empty<IServiceManager>()))
            {
                stream.Position = 0;

                Assert.Equal(ChunkStop.BufferLimit, reader.ReadChunkOrLine(out var chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(40, x), x => Assert.Equal(13, x), x => Assert.Equal(13, x), x => Assert.Equal(13, x), x => Assert.Equal(13, x));

                Assert.Equal(ChunkStop.NewLine, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(13, x));

                Assert.Equal(ChunkStop.EndOfStream, reader.ReadChunkOrLine(out chunk, 5));
                Assert.Collection(chunk, x => Assert.Equal(33, x), x => Assert.Equal(34, x), x => Assert.Equal(40, x), x => Assert.Equal(40, x));
            }
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class DataReaderTest
    {
        private readonly Random _random = new();

        [Fact]
        public async Task CanHandleEmptyStream()
        {
            using var stream = new MemoryStream();
            using var dr = new DataReader(stream);

            Assert.Equal(0, dr.Position);

            Assert.Equal(-1, await dr.PeekByteAsync());
            Assert.Equal(-1, await dr.ReadByteAsync());

            Assert.Equal(0, dr.Position);

            Assert.Equal(-1, await dr.PeekByteAsync());
            Assert.Equal(-1, await dr.ReadByteAsync());

            Assert.Equal(0, dr.Position);

            Assert.Equal(-1, await dr.PeekByteAsync());
            Assert.Equal(-1, await dr.ReadByteAsync());

            Assert.Equal(0, dr.Position);
        }

        [Fact]
        public async Task CanHandleShortStream()
        {
            using var stream = new MemoryStream(new byte[] { 20 });
            using var dr = new DataReader(stream);

            Assert.Equal(0, dr.Position);

            Assert.Equal(20, await dr.PeekByteAsync());
            Assert.Equal(20, await dr.ReadByteAsync());

            Assert.Equal(1, dr.Position);

            Assert.Equal(-1, await dr.PeekByteAsync());
            Assert.Equal(-1, await dr.ReadByteAsync());

            Assert.Equal(1, dr.Position);

            Assert.Equal(-1, await dr.PeekByteAsync());
            Assert.Equal(-1, await dr.ReadByteAsync());

            Assert.Equal(1, dr.Position);
        }

        [Theory]
        [InlineData(500)]
        [InlineData(DataReader.BufferSize)]
        [InlineData(2 * DataReader.BufferSize + 500)]
        public async Task CanHandleVariousBufferSizes(long bufferSize)
        {
            var buffer = new byte[bufferSize];

            _random.NextBytes(buffer);

            using var stream = new MemoryStream(buffer);
            using var dr = new DataReader(stream);

            var expectedPosition = 0;
            int expectedByte;

            do
            {
                Assert.Equal(expectedPosition, dr.Position);

                expectedByte = await dr.PeekByteAsync();

                Assert.Equal(expectedByte, await dr.ReadByteAsync());

                expectedPosition++;
            } while (expectedByte >= 0);

            Assert.Equal(bufferSize, dr.Position);
            Assert.Equal(bufferSize + 1, expectedPosition);
        }
    }
}
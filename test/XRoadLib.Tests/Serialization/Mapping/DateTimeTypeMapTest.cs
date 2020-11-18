using System;
using System.Threading.Tasks;
using XRoadLib.Serialization.Mapping;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class DateTimeTypeMapTest : TypeMapTestBase
    {
        private static readonly DateTimeTypeMap DateTimeTypeMap = new DateTimeTypeMap(SchemaDefinitionProvider.GetSimpleTypeDefinition<DateTime>("dateTime"));
        private static readonly Func<string, Task<object>> DeserializeDateTimeValueAsync = x => DeserializeValueAsync(DateTimeTypeMap, x);

        [Fact]
        public async Task CanDeserializeDatePartOnly()
        {
            var instance = await DeserializeDateTimeValueAsync("2013-08-27");
            Assert.NotNull(instance);

            var dateTime = (DateTime)instance;
            Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
            Assert.Equal(2013, dateTime.Year);
            Assert.Equal(8, dateTime.Month);
            Assert.Equal(27, dateTime.Day);
            Assert.Equal(0, dateTime.Hour);
            Assert.Equal(0, dateTime.Minute);
            Assert.Equal(0, dateTime.Second);
            Assert.Equal(0, dateTime.Millisecond);
        }

        [Fact]
        public async Task CannotDeserializeWrongDateTimeFormat()
        {
            var exception = await Assert.ThrowsAsync<FormatException>(() => DeserializeDateTimeValueAsync("2013-08-27T12:34:61"));
            Assert.Equal("The string '2013-08-27T12:34:61' is not a valid AllXsd value.", exception.Message);
        }

        [Fact]
        public async Task CanDeserializeWithTime()
        {
            var instance = await DeserializeDateTimeValueAsync("2013-08-27T12:34:56");
            Assert.NotNull(instance);

            var dateTime = (DateTime)instance;
            Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
            Assert.Equal(2013, dateTime.Year);
            Assert.Equal(8, dateTime.Month);
            Assert.Equal(27, dateTime.Day);
            Assert.Equal(12, dateTime.Hour);
            Assert.Equal(34, dateTime.Minute);
            Assert.Equal(56, dateTime.Second);
            Assert.Equal(0, dateTime.Millisecond);
        }

        [Fact]
        public async Task DeserializationIgnoresTimeZoneValue()
        {
            var instance = await DeserializeDateTimeValueAsync("2013-08-27T00:00:00+03:00");
            Assert.NotNull(instance);

            var dateTime = (DateTime)instance;
            Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
            Assert.Equal(2013, dateTime.Year);
            Assert.Equal(8, dateTime.Month);
            Assert.Equal(27, dateTime.Day);
            Assert.Equal(0, dateTime.Hour);
            Assert.Equal(0, dateTime.Minute);
            Assert.Equal(0, dateTime.Second);
            Assert.Equal(0, dateTime.Millisecond);
        }

        [Fact]
        public async Task DeserializationIgnoresMillisecondValue()
        {
            var instance = await DeserializeDateTimeValueAsync("2013-08-27T12:34:56.1234+03:00");
            Assert.NotNull(instance);

            var dateTime = (DateTime)instance;
            Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);
            Assert.Equal(2013, dateTime.Year);
            Assert.Equal(8, dateTime.Month);
            Assert.Equal(27, dateTime.Day);
            Assert.Equal(12, dateTime.Hour);
            Assert.Equal(34, dateTime.Minute);
            Assert.Equal(56, dateTime.Second);
            Assert.Equal(0, dateTime.Millisecond);
        }
    }
}
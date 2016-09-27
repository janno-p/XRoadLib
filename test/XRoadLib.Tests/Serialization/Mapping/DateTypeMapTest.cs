using System;
using XRoadLib.Serialization.Mapping;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class DateTypeMapTest : TypeMapTestBase
    {
        private static readonly DateTypeMap dateTypeMap = new DateTypeMap(schemaDefinitionProvider.GetSimpleTypeDefinition<DateTime>("date"));
        private static readonly Func<string, object> deserializeValue = x => DeserializeValue(dateTypeMap, x);

        [Fact]
        public void CanDeserializePlainDate()
        {
            var instance = deserializeValue("2013-08-27");
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
        public void CannotDeserializeWrongFormat()
        {
            var exception = Assert.Throws<FormatException>(() => deserializeValue("2013-08-40"));
            Assert.Equal("String was not recognized as a valid DateTime.", exception.Message);
        }

        [Fact]
        public void CannotDeserializeDateTimeFormat()
        {
            var exception = Assert.Throws<FormatException>(() => deserializeValue("2013-08-04T11:11:11"));
            Assert.Equal("String was not recognized as a valid DateTime.", exception.Message);
        }

        [Fact]
        public void DeserializesUniversalTimezoneToLocalTimezone()
        {
            var expected = TimeZoneInfo.ConvertTime(new DateTime(2013, 8, 27), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            var instance = deserializeValue("2013-08-27Z");
            Assert.NotNull(instance);

            var dateTime = (DateTime)instance;
            Assert.Equal(expected, dateTime);
        }

        [Fact]
        public void DeserializesExplicitTimezoneToLocalTimezone()
        {
            var expected = TimeZoneInfo.ConvertTime(new DateTime(2013, 8, 27, 3, 0, 0), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            var instance = deserializeValue("2013-08-27-03:00");
            Assert.NotNull(instance);

            var dateTime = (DateTime)instance;
            Assert.Equal(expected, dateTime);
        }
    }
}
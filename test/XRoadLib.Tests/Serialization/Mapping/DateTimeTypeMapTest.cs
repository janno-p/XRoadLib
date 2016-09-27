using System;
using XRoadLib.Serialization.Mapping;
using Xunit;

namespace XRoadLib.Tests.Serialization.Mapping
{
    public class DateTimeTypeMapTest : TypeMapTestBase
    {
        private static readonly DateTimeTypeMap dateTimeTypeMap = new DateTimeTypeMap(schemaDefinitionProvider.GetSimpleTypeDefinition<DateTime>("dateTime"));
        private static readonly Func<string, object> deserializeValue = x => DeserializeValue(dateTimeTypeMap, x);

        [Fact]
        public void CanDeserializeDatePartOnly()
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
        public void CannotDeserializeWrongDateTimeFormat()
        {
            var exception = Assert.Throws<FormatException>(() => deserializeValue("2013-08-27T12:34:61"));
            Assert.Equal("The string '2013-08-27T12:34:61' is not a valid AllXsd value.", exception.Message);
        }

        [Fact]
        public void CanDeserializeWithTime()
        {
            var instance = deserializeValue("2013-08-27T12:34:56");
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
        public void DeserializationIgnoresTimeZoneValue()
        {
            var instance = deserializeValue("2013-08-27T00:00:00+03:00");
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
        public void DeserializationIgnoresMillisecondValue()
        {
            var instance = deserializeValue("2013-08-27T12:34:56.1234+03:00");
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
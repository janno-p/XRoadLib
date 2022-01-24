using XRoadLib.Serialization.Mapping;

namespace XRoadLib.Tests.Serialization.Mapping;

public class DateTypeMapTest : TypeMapTestBase
{
    private static readonly DateTypeMap DateTypeMap = new(SchemaDefinitionProvider.GetSimpleTypeDefinition<DateTime>("date"));
    private static readonly Func<string, Task<object>> DeserializeDateValueAsync = x => DeserializeValueAsync(DateTypeMap, x);

    [Fact]
    public async Task CanDeserializePlainDate()
    {
        var instance = await DeserializeDateValueAsync("2013-08-27");
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
    public async Task CannotDeserializeWrongFormat()
    {
        var exception = await Assert.ThrowsAsync<FormatException>(() => DeserializeDateValueAsync("2013-08-40"));
#if NETFRAMEWORK
        Assert.Equal("String was not recognized as a valid DateTime.", exception.Message);
#else
            Assert.Equal("String '2013-08-40' was not recognized as a valid DateTime.", exception.Message);
#endif
    }

    [Fact]
    public async Task CannotDeserializeDateTimeFormat()
    {
        var exception = await Assert.ThrowsAsync<FormatException>(() => DeserializeDateValueAsync("2013-08-04T11:11:11"));
#if NETFRAMEWORK
        Assert.Equal("String was not recognized as a valid DateTime.", exception.Message);
#else
            Assert.Equal("String '2013-08-04T11:11:11' was not recognized as a valid DateTime.", exception.Message);
#endif
    }

    [Fact]
    public async Task DeserializesUniversalTimezoneToLocalTimezone()
    {
        var expected = TimeZoneInfo.ConvertTime(new DateTime(2013, 8, 27), TimeZoneInfo.Utc, TimeZoneInfo.Local);
        var instance = await DeserializeDateValueAsync("2013-08-27Z");
        Assert.NotNull(instance);

        var dateTime = (DateTime)instance;
        Assert.Equal(expected, dateTime);
    }

    [Fact]
    public async Task DeserializesExplicitTimezoneToLocalTimezone()
    {
        var expected = TimeZoneInfo.ConvertTime(new DateTime(2013, 8, 27, 3, 0, 0), TimeZoneInfo.Utc, TimeZoneInfo.Local);
        var instance = await DeserializeDateValueAsync("2013-08-27-03:00");
        Assert.NotNull(instance);

        var dateTime = (DateTime)instance;
        Assert.Equal(expected, dateTime);
    }
}
using XRoadLib.Headers;

namespace XRoadLib.Tests.Protocols.Headers;

public class XRoadIdentifierTest
{
    private const string ProducerName = "producerName";
    private const string ServiceName = "serviceName";
    private const uint Version = 3u;

    private readonly string _fullName = $"{ProducerName}.{ServiceName}.v{Version}";

    [Theory]
    [InlineData(null, null, null, null, "")]
    [InlineData("ee-dev", null, null, null, "ee-dev//")]
    [InlineData(null, "GOV", null, null, "/GOV/")]
    [InlineData(null, null, "11111111", null, "11111111")]
    [InlineData(null, null, null, "test", "///test")]
    [InlineData("ee-dev", null, null, "test", "ee-dev///test")]
    [InlineData("ee-dev", "GOV", "11111111", "test", "ee-dev/GOV/11111111/test")]
    public void XRoadClientIdentifierTest(string instance, string memberClass, string memberCode, string subsystemCode, string expectedValue)
    {
        var id = new XRoadClientIdentifier
        {
            XRoadInstance = instance,
            MemberClass = memberClass,
            MemberCode = memberCode,
            SubsystemCode = subsystemCode
        };
        Assert.Equal(expectedValue, id.ToString());
    }

    [Theory]
    [InlineData(null, null, "/")]
    [InlineData("ee-dev", null, "ee-dev/")]
    [InlineData(null, "test", "/test")]
    [InlineData("ee-dev", "test", "ee-dev/test")]
    public void XRoadCentralServiceIdentifierTest(string instance, string serviceCode, string expectedValue)
    {
        var id = new XRoadCentralServiceIdentifier
        {
            XRoadInstance = instance,
            ServiceCode = serviceCode
        };
        Assert.Equal(expectedValue, id.ToString());
    }

    [Theory]
    [InlineData(null, null, null, null, null, null, "/")]
    [InlineData(null, null, null, null, null, "v1", "//v1")]
    [InlineData(null, null, null, null, "test", null, "/test")]
    [InlineData(null, null, null, null, "test", "v1", "/test/v1")]
    [InlineData(null, null, null, "generic-producer", null, null, "generic-producer/")]
    [InlineData(null, null, null, "generic", "test", "v1", "generic/test/v1")]
    [InlineData(null, null, null, "generic", null, "v1", "generic//v1")]
    [InlineData(null, null, null, "generic", "test", null, "generic/test")]
    [InlineData("ee-dev", null, null, null, null, null, "ee-dev////")]
    [InlineData(null, "GOV", null, null, null, null, "/GOV///")]
    [InlineData(null, null, "11111111", null, null, null, "//11111111//")]
    [InlineData("ee-dev", "GOV", "11111111", "generic", "test", "v1", "ee-dev/GOV/11111111/generic/test/v1")]
    [InlineData("ee-dev", "GOV", "11111111", null, "test", "v1", "ee-dev/GOV/11111111//test/v1")]
    public void XRoadServiceIdentifierTest(string instance, string memberClass, string memberCode, string subsystemCode, string serviceCode, string serviceVersion, string expectedValue)
    {
        var id = new XRoadServiceIdentifier
        {
            XRoadInstance = instance,
            MemberClass = memberClass,
            MemberCode = memberCode,
            SubsystemCode = subsystemCode,
            ServiceCode = serviceCode,
            ServiceVersion = serviceVersion
        };
        Assert.Equal(expectedValue, id.ToString());
    }

    [Fact]
    public void CanParseFullServiceName()
    {
        var xsn = XRoadServiceIdentifier.FromString(_fullName);
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal(ServiceName, xsn.ServiceCode);
        Assert.Equal(Version, xsn.Version);
    }

    [Fact]
    public void CanParseNullValue()
    {
        var xsn = XRoadServiceIdentifier.FromString(null);
        Assert.NotNull(xsn);
        Assert.Null(xsn.SubsystemCode);
        Assert.Null(xsn.ServiceCode);
        Assert.Null(xsn.Version);
    }

    [Fact]
    public void CanParseEmptyString()
    {
        var xsn = XRoadServiceIdentifier.FromString("");
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal("", xsn.ServiceCode);
        Assert.Null(xsn.Version);
    }

    [Fact]
    public void InitializeWithoutProducerInFullName()
    {
        const string withoutProducer = "serviceName.v1";
        var xsn = XRoadServiceIdentifier.FromString(withoutProducer);
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal("serviceName", xsn.ServiceCode);
        Assert.Equal(1u, xsn.Version);
        Assert.Equal("serviceName.v1", xsn.ToFullName());
        Assert.Equal("/serviceName/v1", xsn.ToString());
    }

    [Fact]
    public void InitializeWithoutProducerAndVersionInFullName()
    {
        const string withoutProducerAndVersion = "serviceName";
        var xsn = XRoadServiceIdentifier.FromString(withoutProducerAndVersion);
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal("serviceName", xsn.ServiceCode);
        Assert.Null(xsn.Version);
        Assert.Equal("serviceName", xsn.ToFullName());
        Assert.Equal("/serviceName", xsn.ToString());
    }

    [Fact]
    public void InitializeWithoutVersionInFullName()
    {
        const string withoutVersion = "producer.listMethods";
        var xsn = XRoadServiceIdentifier.FromString(withoutVersion);
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal("listMethods", xsn.ServiceCode);
        Assert.Null(xsn.Version);
        Assert.Equal("listMethods", xsn.ToFullName());
        Assert.Equal("/listMethods", xsn.ToString());
    }

    [Fact]
    public void InitializeWithoutServiceNameInFullName()
    {
        const string withoutServiceName = "producer..v2";
        var xsn = XRoadServiceIdentifier.FromString(withoutServiceName);
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal("", xsn.ServiceCode);
        Assert.Equal((uint?)2, xsn.Version);
        Assert.Equal(".v2", xsn.ToFullName());
        Assert.Equal("//v2", xsn.ToString());
    }

    [Fact]
    public void InitializeWithVersion0()
    {
        var fullName = $"{ProducerName}.{ServiceName}.v0";
        var xsn = XRoadServiceIdentifier.FromString(fullName);
        Assert.Null(xsn.SubsystemCode);
        Assert.Equal("serviceName", xsn.ServiceCode);
        Assert.Equal((uint?)0, xsn.Version);
        Assert.Equal("serviceName.v0", xsn.ToFullName());
        Assert.Equal("/serviceName/v0", xsn.ToString());
    }

    [Fact]
    public void ConvertToStringWithAllPartsAssigned()
    {
        var xsn = XRoadServiceIdentifier.FromString(_fullName);
        Assert.Equal("serviceName.v3", xsn.ToFullName());
        Assert.Equal("/serviceName/v3", xsn.ToString());
    }
}
using XRoadLib.Protocols.Headers;
using Xunit;

namespace XRoadLib.Tests.Protocols.Headers
{
    public class XRoadIdentifierTest
    {
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

/*
[<TestFixture>]
module XRoadServiceNameTest =
    let [<Literal>] producerName = "producerName"
    let [<Literal>] serviceName = "serviceName"
    let [<Literal>] version = 3u

    let fullName = sprintf "%s.%s.v%d" producerName serviceName version

    [<Test>]
    let ``can parse full service name`` () =
        let xsn = XRoadServiceIdentifier.FromString(fullName)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal serviceName
        xsn.Version |> should equal version

    [<Test>]
    let ``initialize with null value`` () =
        let xsn = XRoadServiceIdentifier.FromString(null)
        xsn |> should not' (be Null)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should be Null
        xsn.Version |> should be Null

    [<Test>]
    let ``initialize with empty string`` () =
        let xsn = XRoadServiceIdentifier.FromString("")
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal ""
        xsn.Version |> should be Null

    [<Test>]
    let ``initialize without producer in full name`` () =
        let withoutProducer = "serviceName.v1"
        let xsn = XRoadServiceIdentifier.FromString(withoutProducer)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "serviceName"
        xsn.Version |> should equal (Nullable(1u))
        xsn.ToFullName() |> should equal "serviceName.v1"
        xsn.ToString() |> should equal "/serviceName/v1"

    [<Test>]
    let ``initialize without producer and version in full name`` () =
        let withoutProducerAndVersion = "serviceName"
        let xsn = XRoadServiceIdentifier.FromString(withoutProducerAndVersion)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "serviceName"
        xsn.Version |> should be Null
        xsn.ToFullName() |> should equal "serviceName"
        xsn.ToString() |> should equal "/serviceName"

    [<Test>]
    let ``initialize without version in full name`` () =
        let withoutVersion = "producer.listMethods"
        let xsn = XRoadServiceIdentifier.FromString(withoutVersion)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "listMethods"
        xsn.Version |> should be Null
        xsn.ToFullName() |> should equal "listMethods"
        xsn.ToString() |> should equal "/listMethods"

    [<Test>]
    let ``initialize without serviceName in full name`` () =
        let withoutServiceName = "producer..v2"
        let xsn = XRoadServiceIdentifier.FromString(withoutServiceName)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal ""
        xsn.Version |> should equal (Nullable(2u))
        xsn.ToFullName() |> should equal ".v2"
        xsn.ToString() |> should equal "//v2"

    [<Test>]
    let ``initialize with version 0`` () =
        let fullName = sprintf "%s.%s.v%d" producerName serviceName 0u
        let xsn = XRoadServiceIdentifier.FromString(fullName)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "serviceName"
        xsn.Version |> should equal (Nullable(0u))
        xsn.ToFullName() |> should equal "serviceName.v0"
        xsn.ToString() |> should equal "/serviceName/v0"

    [<Test>]
    let ``convert to string with all parts assigned`` () =
        let xsn = XRoadServiceIdentifier.FromString(fullName)
        xsn.ToFullName() |> should equal "serviceName.v3"
        xsn.ToString() |> should equal "/serviceName/v3"


*/
    }
}
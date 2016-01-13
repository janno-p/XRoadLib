namespace XRoadLib.Tests

open NUnit.Framework
open System
open XRoadLib
open XRoadLib.Extensions

[<TestFixture>]
module NamespaceHelperTest =
    [<TestCase("test", -1, ExpectedException = typeof<ArgumentOutOfRangeException >, ExpectedMessage = "Specified argument was out of the range of valid values.", MatchType = MessageMatch.Contains)>]
    [<TestCase("test", XRoadProtocol.Undefined, ExpectedException = typeof<ArgumentOutOfRangeException >, ExpectedMessage = "Specified argument was out of the range of valid values.", MatchType = MessageMatch.Contains)>]
    [<TestCase("test", XRoadProtocol.Version20, Result = "http://producers.test.xtee.riik.ee/producer/test")>]
    [<TestCase("test", XRoadProtocol.Version31, Result = "http://test.x-road.ee/producer/")>]
    [<TestCase("test", XRoadProtocol.Version40, Result = "http://test.x-road.eu")>]
    let ``get producer namespace for protocol version`` (producerName, protocol: XRoadProtocol) =
        protocol.GetProducerNamespace(producerName)

    [<TestCase(-1, ExpectedException = typeof<ArgumentOutOfRangeException >, ExpectedMessage = "Specified argument was out of the range of valid values.", MatchType = MessageMatch.Contains)>]
    [<TestCase(XRoadProtocol.Undefined, ExpectedException = typeof<ArgumentOutOfRangeException >, ExpectedMessage = "Specified argument was out of the range of valid values.", MatchType = MessageMatch.Contains)>]
    [<TestCase(XRoadProtocol.Version20, Result = "http://x-tee.riik.ee/xsd/xtee.xsd")>]
    [<TestCase(XRoadProtocol.Version31, Result = "http://x-road.ee/xsd/x-road.xsd")>]
    [<TestCase(XRoadProtocol.Version40, Result = "http://x-road.eu/xsd/xroad.xsd")>]
    let ``get X-Road namespace for protocol version`` (protocol: XRoadProtocol) =
        protocol.GetNamespace()

    [<TestCase(-1, ExpectedException = typeof<ArgumentOutOfRangeException >, ExpectedMessage = "Specified argument was out of the range of valid values.", MatchType = MessageMatch.Contains)>]
    [<TestCase(XRoadProtocol.Undefined, ExpectedException = typeof<ArgumentOutOfRangeException >, ExpectedMessage = "Specified argument was out of the range of valid values.", MatchType = MessageMatch.Contains)>]
    [<TestCase(XRoadProtocol.Version20, Result = "xtee")>]
    [<TestCase(XRoadProtocol.Version31, Result = "xrd")>]
    [<TestCase(XRoadProtocol.Version40, Result = "xrd")>]
    let ``get prefix for protocol version`` (protocol: XRoadProtocol) =
        protocol.GetPrefix()

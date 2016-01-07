namespace XRoadLib.Tests

open NUnit.Framework
open System
open XRoadLib

[<TestFixture>]
module NamespaceHelperTest =
    [<TestCase("test", -1, ExpectedException = typeof<ArgumentException>, ExpectedMessage = "Invalid protocol version.", MatchType = MessageMatch.Contains)>]
    [<TestCase("test", XRoadProtocol.Version20, Result = "http://producers.test.xtee.riik.ee/producer/test")>]
    [<TestCase("test", XRoadProtocol.Version31, Result = "http://test.x-road.ee/producer/")>]
    let ``get producer namespace for protocol version`` (producerName, protocol) =
        NamespaceHelper.GetProducerNamespace(producerName, protocol)

    [<TestCase(-1, ExpectedException = typeof<ArgumentException>, ExpectedMessage = "Invalid protocol version.", MatchType = MessageMatch.Contains)>]
    [<TestCase(XRoadProtocol.Version20, Result = "http://x-tee.riik.ee/xsd/xtee.xsd")>]
    [<TestCase(XRoadProtocol.Version31, Result = "http://x-road.ee/xsd/x-road.xsd")>]
    let ``get X-Road namespace for protocol version`` (protocol) =
        NamespaceHelper.GetXRoadNamespace(protocol)

    [<TestCase(-1, ExpectedException = typeof<ArgumentException>, ExpectedMessage = "Invalid protocol version.", MatchType = MessageMatch.Contains)>]
    [<TestCase(XRoadProtocol.Version20, Result = "xtee")>]
    [<TestCase(XRoadProtocol.Version31, Result = "xrd")>]
    let ``get prefix for protocol version`` (protocol) =
        PrefixHelper.GetXRoadPrefix(protocol)

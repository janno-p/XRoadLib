module XRoadLib.Tests

open XRoadLib
open NUnit.Framework

[<Test>]
let ``creates correct producer namespace for protocol version 3.1`` () =
    let result = NamespaceHelper.GetProducerNamespace("test", XRoadProtocol.Version31)
    Assert.AreEqual("http://test.x-road.ee/producer/", result)

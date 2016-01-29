module Globals

let XRoadProtocol20 = XRoadLib.Protocols.XRoad20Protocol("test-producer", "http://producers.test-producer.xtee.riik.ee/producer/test-producer")
let XRoadProtocol31 = XRoadLib.Protocols.XRoad31Protocol("test-producer", "http://test-producer.x-road.ee/producer/")
let XRoadProtocol40 = XRoadLib.Protocols.XRoad40Protocol("http://test-producer.x-road.eu/")

do
    let assembly = typeof<XRoadLib.Tests.Contract.Class1>.Assembly
    XRoadProtocol20.SetContractAssembly(assembly, 1u, 2u)
    XRoadProtocol31.SetContractAssembly(assembly, 1u, 2u)
    XRoadProtocol40.SetContractAssembly(assembly, 1u, 2u)

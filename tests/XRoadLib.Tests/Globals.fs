module Globals

open XRoadLib.Schema

let XRoadProtocol20 = XRoadLib.Tests.Contract.Configuration.CustomXRoad20Protocol.Instance
let XRoadProtocol31 = XRoadLib.Tests.Contract.Configuration.CustomXRoad31Protocol.Instance

let XRoadProtocol40 =
    let protocol = XRoadLib.Protocols.XRoad40Protocol("http://test-producer.x-road.eu/")
    protocol.SetContractAssembly(typeof<XRoadLib.Tests.Contract.Class1>.Assembly, null, 1u, 2u)
    protocol

type TestDefinition (typ) =
    interface IContentDefinition with
        member val UseXop = false
        member val RuntimeType = typ
        member val TypeName = null
        member val ArrayItemDefinition = null
        member val MergeContent = false

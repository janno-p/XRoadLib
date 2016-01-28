namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open XRoadLib
open XRoadLib.Protocols.Headers

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
        xsn.ToString() |> should equal "SERVICE:XX/_/_/serviceName/v1"

    [<Test>]
    let ``initialize without producer and version in full name`` () =
        let withoutProducerAndVersion = "serviceName"
        let xsn = XRoadServiceIdentifier.FromString(withoutProducerAndVersion)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "serviceName"
        xsn.Version |> should be Null
        xsn.ToFullName() |> should equal "serviceName"
        xsn.ToString() |> should equal "SERVICE:XX/_/_/serviceName"

    [<Test>]
    let ``initialize without version in full name`` () =
        let withoutVersion = "producer.listMethods"
        let xsn = XRoadServiceIdentifier.FromString(withoutVersion)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "listMethods"
        xsn.Version |> should be Null
        xsn.ToFullName() |> should equal "listMethods"
        xsn.ToString() |> should equal "SERVICE:XX/_/_/listMethods"

    [<Test>]
    let ``initialize without serviceName in full name`` () =
        let withoutServiceName = "producer..v2"
        let xsn = XRoadServiceIdentifier.FromString(withoutServiceName)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal ""
        xsn.Version |> should equal (Nullable(2u))
        xsn.ToFullName() |> should equal ".v2"
        xsn.ToString() |> should equal "SERVICE:XX/_/_/_/v2"

    [<Test>]
    let ``initialize with version 0`` () =
        let fullName = sprintf "%s.%s.v%d" producerName serviceName 0u
        let xsn = XRoadServiceIdentifier.FromString(fullName)
        xsn.SubsystemCode |> should be Null
        xsn.ServiceCode |> should equal "serviceName"
        xsn.Version |> should equal (Nullable(0u))
        xsn.ToFullName() |> should equal "serviceName.v0"
        xsn.ToString() |> should equal "SERVICE:XX/_/_/serviceName/v0"

    [<Test>]
    let ``convert to string with all parts assigned`` () =
        let xsn = XRoadServiceIdentifier.FromString(fullName)
        xsn.ToFullName() |> should equal "serviceName.v3"
        xsn.ToString() |> should equal "SERVICE:XX/_/_/serviceName/v3"

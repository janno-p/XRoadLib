namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open XRoadLib
open XRoadLib.Header

[<TestFixture>]
module XRoadServiceNameTest =
    let [<Literal>] producerName = "producerName"
    let [<Literal>] serviceName = "serviceName"
    let [<Literal>] version = 3u

    let fullName = sprintf "%s.%s.v%d" producerName serviceName version

    [<Test>]
    let ``can parse full service name`` () =
        let xsn = XRoadServiceIdentifier.FromString(fullName)
        xsn.Producer |> should equal producerName
        xsn.Method |> should equal serviceName
        xsn.Version |> should equal version

    [<Test>]
    let ``initialize each part in constructor`` () =
        let xsn = XRoadServiceName(producerName, serviceName, Nullable(version))
        xsn.Producer |> should equal producerName
        xsn.Method |> should equal serviceName
        xsn.Version |> should equal version

    [<Test>]
    let ``initialize with null value`` () =
        let xsn = XRoadServiceName(null)
        xsn.Producer |> should be Null
        xsn.Method |> should be Null
        xsn.Version |> should be Null

    [<Test>]
    let ``initialize with empty string`` () =
        let xsn = XRoadServiceName("")
        xsn.Producer |> should be Null
        xsn.Method |> should equal ""
        xsn.Version |> should be Null

    [<Test>]
    let ``initialize without producer in full name`` () =
        let withoutProducer = "serviceName.v1"
        let xsn = XRoadServiceName(withoutProducer)
        xsn.Producer |> should be Null
        xsn.Method |> should equal "serviceName"
        xsn.Version |> should equal (Nullable(1u))
        xsn.ToString() |> should equal withoutProducer

    [<Test>]
    let ``initialize without producer and version in full name`` () =
        let withoutProducerAndVersion = "serviceName"
        let xsn = XRoadServiceName(withoutProducerAndVersion)
        xsn.Producer |> should be Null
        xsn.Method |> should equal "serviceName"
        xsn.Version |> should be Null
        xsn.ToString() |> should equal withoutProducerAndVersion

    [<Test>]
    let ``initialize without version in full name`` () =
        let withoutVersion = "producer.listMethods"
        let xsn = XRoadServiceName(withoutVersion)
        xsn.Producer |> should equal "producer"
        xsn.Method |> should equal "listMethods"
        xsn.Version |> should be Null
        xsn.ToString() |> should equal withoutVersion

    [<Test>]
    let ``initialize without serviceName in full name`` () =
        let withoutServiceName = "producer..v2"
        let xsn = XRoadServiceName(withoutServiceName)
        xsn.Producer |> should equal "producer"
        xsn.Method |> should equal ""
        xsn.Version |> should equal (Nullable(2u))
        xsn.ToString() |> should equal withoutServiceName

    [<Test>]
    let ``initialize with version 0`` () =
        let fullName = sprintf "%s.%s.v%d" producerName serviceName 0u
        let xsn = XRoadServiceName(fullName)
        xsn.Producer |> should equal "producerName"
        xsn.Method |> should equal "serviceName"
        xsn.Version |> should equal (Nullable(0u))
        xsn.ToString() |> should equal fullName

    [<Test>]
    let ``convert to string with all parts assigned`` () =
        let xsn = XRoadServiceName(fullName)
        xsn.ToString() |> should equal fullName

    [<Test>]
    let ``initialize without version number`` () =
        let xsn = XRoadServiceName(producerName, serviceName)
        xsn.Producer |> should equal "producerName"
        xsn.Method |> should equal "serviceName"
        xsn.Version |> should be Null
        xsn.ToString() |> should equal "producerName.serviceName"

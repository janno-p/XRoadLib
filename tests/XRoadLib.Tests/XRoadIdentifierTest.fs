namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open XRoadLib.Protocols.Headers

[<TestFixture>]
module XRoadClientIdentifierTest =
    let [<Test>] ``identifier without content`` () =
        let id = XRoadClientIdentifier()
        id.ToString() |> should equal ""

    let [<Test>] ``only X-Road instance`` () =
        let id = XRoadClientIdentifier(XRoadInstance = "ee-dev")
        id.ToString() |> should equal "ee-dev//"

    let [<Test>] ``only member class`` () =
        let id = XRoadClientIdentifier(MemberClass = "GOV")
        id.ToString() |> should equal "/GOV/"

    let [<Test>] ``old header identifier (member code only)`` () =
        let id = XRoadClientIdentifier(MemberCode = "11111111")
        id.ToString() |> should equal "11111111"

    let [<Test>] ``only subsystem`` () =
        let id = XRoadClientIdentifier(SubsystemCode = "test")
        id.ToString() |> should equal "///test"

    let [<Test>] ``has parts of new X-Road identifier and subsystem code`` () =
        let id = XRoadClientIdentifier(XRoadInstance = "ee-dev", SubsystemCode = "test")
        id.ToString() |> should equal "ee-dev///test"

    let [<Test>] ``full identifier`` () =
        let id = XRoadClientIdentifier(XRoadInstance = "ee-dev", MemberClass = "GOV", MemberCode = "11111111", SubsystemCode = "test")
        id.ToString() |> should equal "ee-dev/GOV/11111111/test"

[<TestFixture>]
module XRoadCentralServiceIdentifierTest =
    let [<Test>] ``empty identifier`` () =
        XRoadCentralServiceIdentifier().ToString() |> should equal "/"

    let [<Test>] ``only X-Road instance`` () =
        XRoadCentralServiceIdentifier(XRoadInstance = "ee-dev").ToString() |> should equal "ee-dev/"

    let [<Test>] ``only service code`` () =
        XRoadCentralServiceIdentifier(ServiceCode = "test").ToString() |> should equal "/test"

    let [<Test>] ``full identifier`` () =
        XRoadCentralServiceIdentifier(XRoadInstance = "ee-dev", ServiceCode = "test").ToString() |> should equal "ee-dev/test"

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

[<TestFixture>]
module XRoadServiceIdentifierTest =
    let [<Test>] ``empty identifier`` () =
        XRoadServiceIdentifier().ToString() |> should equal "/"

    let [<Test>] ``only version`` () =
        XRoadServiceIdentifier(ServiceVersion = "v1").ToString() |> should equal "//v1"

    let [<Test>] ``only service code`` () =
        XRoadServiceIdentifier(ServiceCode = "test").ToString() |> should equal "/test"

    let [<Test>] ``service code and version`` () =
        XRoadServiceIdentifier(ServiceCode = "test", ServiceVersion = "v1").ToString() |> should equal "/test/v1"

    let [<Test>] ``subsystem code only`` () =
        XRoadServiceIdentifier(SubsystemCode = "generic-producer").ToString() |> should equal "generic-producer/"

    let [<Test>] ``subsystem code with service and version`` () =
        XRoadServiceIdentifier(SubsystemCode = "generic", ServiceCode = "test", ServiceVersion = "v1").ToString() |> should equal "generic/test/v1"

    let [<Test>] ``subsystem code with version`` () =
        XRoadServiceIdentifier(SubsystemCode = "generic", ServiceVersion = "v1").ToString() |> should equal "generic//v1"

    let [<Test>] ``subsystem code with service`` () =
        XRoadServiceIdentifier(SubsystemCode = "generic", ServiceCode = "test").ToString() |> should equal "generic/test"

    let [<Test>] ``X-Road instance only`` () =
        XRoadServiceIdentifier(XRoadInstance = "ee-dev").ToString() |> should equal "ee-dev////"

    let [<Test>] ``member class only`` () =
        XRoadServiceIdentifier(MemberClass = "GOV").ToString() |> should equal "/GOV///"

    let [<Test>] ``member code only`` () =
        XRoadServiceIdentifier(MemberCode = "11111111").ToString() |> should equal "//11111111//"

    let [<Test>] ``full identifier`` () =
        XRoadServiceIdentifier(XRoadInstance = "ee-dev", MemberClass = "GOV", MemberCode = "11111111", SubsystemCode = "generic", ServiceCode = "test", ServiceVersion = "v1").ToString() |> should equal "ee-dev/GOV/11111111/generic/test/v1"

    let [<Test>] ``full identifier without subsystem code`` () =
        XRoadServiceIdentifier(XRoadInstance = "ee-dev", MemberClass = "GOV", MemberCode = "11111111", ServiceCode = "test", ServiceVersion = "v1").ToString() |> should equal "ee-dev/GOV/11111111//test/v1"

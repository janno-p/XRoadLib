namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open XRoadLib
open XRoadLib.Extensions

[<TestFixture>]
module AssemblyExtensionsTest =
    type Marker = class end

    [<Test>]
    let ``find missing X-Road producer name`` () =
        typeof<Marker>.Assembly.FindProducerName(XRoadProtocol.Version20) |> should be Null

    [<Test>]
    let ``get missing X-Road producer name`` () =
        TestDelegate(fun _ -> typeof<Marker>.Assembly.GetProducerName(XRoadProtocol.Version31) |> ignore)
        |> should (throwWithMessage @"Unable to extract producer name from contract assembly `XRoadLib.Tests` for protocol `Version31`.") typeof<Exception>

    [<Test>]
    let ``find existing X-Road producer name`` () =
        typeof<XRoadLib.Tests.Contract.Class1>.Assembly.FindProducerName(XRoadProtocol.Version20) |> should equal "test-producer"

    [<Test>]
    let ``get existing X-Road producer name`` () =
        typeof<XRoadLib.Tests.Contract.Class1>.Assembly.GetProducerName(XRoadProtocol.Version31) |> should equal "test-producer"

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
        XRoadProtocol.Version20.FindProducerName(typeof<Marker>.Assembly) |> should be Null

    [<Test>]
    let ``get missing X-Road producer name`` () =
        TestDelegate(fun _ -> XRoadProtocol.Version20.GetProducerName(typeof<Marker>.Assembly) |> ignore)
        |> should (throwWithMessage @"Assembly `XRoadLib.Tests` does not offer contract for X-Road messaging protocol version `Version20`.") typeof<Exception>

    [<Test>]
    let ``find existing X-Road producer name`` () =
        XRoadProtocol.Version20.FindProducerName(typeof<XRoadLib.Tests.Contract.Class1>.Assembly) |> should equal "test-producer"

    [<Test>]
    let ``get existing X-Road producer name`` () =
        XRoadProtocol.Version20.GetProducerName(typeof<XRoadLib.Tests.Contract.Class1>.Assembly) |> should equal "test-producer"

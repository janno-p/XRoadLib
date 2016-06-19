namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open XRoadLib.Soap

[<TestFixture>]
module SoapFaultCodesTest =
    [<Test>]
    let ``client fault has correct code`` () =
        let faultCode = ClientFaultCode("message content")
        faultCode.Value |> should equal "Client.message content"

    [<Test>]
    let ``server fault has correct code`` () =
        let faultCode = ServerFaultCode("server message content")
        faultCode.Value |> should equal "Server.server message content"

    [<Test>]
    let ``without message code`` () =
        let faultCode = ClientFaultCode()
        faultCode.Value |> should equal "Client"

    [<Test>]
    let ``internal server error`` () =
        let faultCode = ServerFaultCode.InternalError
        faultCode.Value |> should equal "Server.InternalError"

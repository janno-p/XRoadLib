namespace XRoadLib.Tests.Serialization
{
/*
[<TestFixture>]
module XRoadHeader31Tests =
    let parseHeader xml = Util.parseHeader xml NamespaceConstants.XROAD

    let testHeader name value =
        let xhr,_,pr = parseHeader (sprintf "<xrd:%s>%s</xrd:%s>" name value name)
        xhr |> should not' (be Null)
        xhr |> should be instanceOfType<IXRoadHeader31>
        pr |> should be (sameAs Globals.XRoadProtocol31)
        xhr, xhr :?> IXRoadHeader31

    [<Test>]
    let ``can parse empty element`` () =
        let xhr,_,pr = parseHeader "<xrd:userId />"
        xhr |> should not' (be Null)
        xhr.UserId |> should equal ""

    [<Test>]
    let ``can parse consumer value`` () =
        let xhr,xhr3 = testHeader "consumer" "12345"
        xhr.Client |> should not' (be Null)
        xhr.Client.MemberCode |> should equal "12345"
        xhr3.Consumer |> should equal "12345"

    [<Test>]
    let ``can parse producer value`` () =
        let xhr,xhr3 = testHeader "producer" "andmekogu"
        xhr.Service |> should not' (be Null)
        xhr.Service.SubsystemCode |> should equal "andmekogu"
        xhr3.Producer |> should equal "andmekogu"

    [<Test>]
    let ``can parse userId value`` () =
        let xhr,_ = testHeader "userId" "Kasutaja"
        xhr.UserId |> should equal "Kasutaja"

    [<Test>]
    let ``can parse id value`` () =
        let xhr,_ = testHeader "id" "hash"
        xhr.Id |> should equal "hash"

    [<Test>]
    let ``can parse service value`` () =
        let xhr,xhr3 = testHeader "service" "producer.testService.v5"
        xhr.Service |> should not' (be Null)
        xhr.Service.SubsystemCode |> should be Null
        xhr.Service.ServiceCode |> should equal "testService"
        xhr.Service.ServiceVersion |> should equal "v5"
        xhr.Service.Version |> should equal (System.Nullable(5u))
        xhr3.Service |> should equal "testService.v5"

    [<Test>]
    let ``can parse issue value`` () =
        let xhr,_ = testHeader "issue" "toimik"
        xhr.Issue |> should equal "toimik"

    [<Test>]
    let ``can parse unit value`` () =
        let _,xhr3 = testHeader "unit" "yksus"
        xhr3.Unit |> should equal "yksus"

    [<Test>]
    let ``can parse position value`` () =
        let _,xhr3 = testHeader "position" "ametikoht"
        xhr3.Position |> should equal "ametikoht"

    [<Test>]
    let ``can parse userName value`` () =
        let _,xhr3 = testHeader "userName" "Kuldar"
        xhr3.UserName |> should equal "Kuldar"

    [<Test>]
    let ``can parse async "1" value`` () =
        let _,xhr3 = testHeader "async" "1"
        xhr3.Async |> should be True

    [<Test>]
    let ``can parse async "true" value`` () =
        let _,xhr3 = testHeader "async" "true"
        xhr3.Async |> should be True

    [<Test>]
    let ``can parse async "false" value`` () =
        let _,xhr3 = testHeader "async" "false"
        xhr3.Async |> should be False

    [<Test>]
    let ``can parse async "0" value`` () =
        let _,xhr3 = testHeader "async" "0"
        xhr3.Async |> should be False

    [<Test>]
    let ``can parse async "" value`` () =
        let _,xhr3 = testHeader "async" ""
        xhr3.Async |> should be False

    [<Test>]
    let ``can parse authenticator value`` () =
        let _,xhr3 = testHeader "authenticator" "Juss"
        xhr3.Authenticator |> should equal "Juss"

    [<Test>]
    let ``can parse paid value`` () =
        let _,xhr3 = testHeader "paid" "just"
        xhr3.Paid |> should equal "just"

    [<Test>]
    let ``can parse encrypt value`` () =
        let _,xhr3 = testHeader "encrypt" "sha1"
        xhr3.Encrypt |> should equal "sha1"

    [<Test>]
    let ``can parse encryptCert value`` () =
        let _,xhr3 = testHeader "encryptCert" "bibopp"
        xhr3.EncryptCert |> should equal "bibopp"

    [<Test>]
    let ``can parse encrypted value`` () =
        let _,xhr3 = testHeader "encrypted" "sha1"
        xhr3.Encrypted |> should equal "sha1"

    [<Test>]
    let ``can parse encryptedCert value`` () =
        let _,xhr3 = testHeader "encryptedCert" "bibopp"
        xhr3.EncryptedCert |> should equal "bibopp"
*/
}
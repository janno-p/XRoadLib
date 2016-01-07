namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System.IO
open System.Linq
open System.Xml.Linq
open XRoadLib

[<TestFixture>]
module ProducerDefinitionTest =
    let xn nm = XName.Get(nm)
    let wsdl nm = XName.Get(nm, NamespaceHelper.WSDL)
    let soap nm = XName.Get(nm, NamespaceHelper.SOAP)
    let xrd nm = XName.Get(nm, NamespaceHelper.XROAD)
    let xtee nm = XName.Get(nm, NamespaceHelper.XTEE)
    let xml nm = XName.Get(nm, NamespaceHelper.XML)

    let attributeValueShouldEqual value (a: XAttribute) =
        a |> should not' (be Null)
        a.Value |> should equal value

    let shouldMatchInCommonParts (doc: XContainer) (xrdns: string -> XName) =
        let root = doc.Elements(wsdl "definitions").SingleOrDefault()
        root |> should not' (be Null)

        let service = root.Elements(wsdl "service").SingleOrDefault()
        service |> should not' (be Null)
        service.Attributes().Count() |> should equal 1
        service.Attribute(xn "name") |> attributeValueShouldEqual "testService"
        service.Elements().Count() |> should equal 1

        let port = service.Elements(wsdl "port").SingleOrDefault()
        port |> should not' (be Null)
        port.Attributes().Count() |> should equal 2
        port.Attribute(xn "name") |> attributeValueShouldEqual "testPort"
        port.Attribute(xn "binding") |> attributeValueShouldEqual "testBinding"

        let okValues = [soap "address"; xrdns "address"; xrdns "title"]
        port.Elements() |> Seq.filter (fun e -> okValues |> List.exists((=) e.Name) |> not) |> should be Empty

        let soapAddress = port.Elements(soap "address").SingleOrDefault()
        soapAddress |> should not' (be Null)
        soapAddress.IsEmpty |> should equal true
        soapAddress.Attributes().Count() |> should equal 1

        port

    let getDocument (definition: ProducerDefinition) =
        use stream = new MemoryStream()
        definition.SaveTo(stream)
        stream.Position <- 0L
        XDocument.Load(stream)

    [<Test>]
    let ``empty service description`` () =
        let doc = ProducerDefinition(XRoadProtocol.Version31, "test", 1u) |> getDocument
        let port = shouldMatchInCommonParts doc xrd

        let address = port.Elements(xrd "address").SingleOrDefault()
        address |> should not' (be Null)
        address.IsEmpty |> should equal true
        address.Attributes().Count() |> should equal 1
        address.Attribute(xn "producer") |> attributeValueShouldEqual "test"

        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual ""

    [<Test>]
    let ``empty legacy format service description`` () =
        let doc = ProducerDefinition(XRoadProtocol.Version20, "test", 1u) |> getDocument
        let port = shouldMatchInCommonParts doc xtee

        let address = port.Elements(xtee "address").SingleOrDefault()
        address |> should not' (be Null)
        address.IsEmpty |> should equal true
        address.Attributes().Count() |> should equal 1
        address.Attribute(xn "producer") |> attributeValueShouldEqual "test"

        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual ""

    [<Test>]
    let ``should define service location if given`` () =
        let url = "http://securityserveruri"
        let definition = ProducerDefinition(XRoadProtocol.Version31, "test", 1u)
        definition.set_Location(url)
        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xrd
        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual url

    [<Test>]
    let ``should define service title`` () =
        let title1 = "Ilma keeleta palun"
        let title2 = "Portugalikeelne loba ..."

        let definition = ProducerDefinition(XRoadProtocol.Version31, "test", 1u)
        definition.Title.Add("", title1)
        definition.Title.Add("pt", title2)

        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xrd

        let titleElements = port.Elements(xrd "title") |> List.ofSeq
        titleElements.Length |> should equal 2

        let yesCode, noCode = titleElements |> List.partition (fun x -> x.Attributes().Any())

        yesCode.Length |> should equal 1
        yesCode.Head |> should not' (be Null)
        yesCode.Head.Value |> should equal title2
        yesCode.Head.Attribute(xml "lang") |> attributeValueShouldEqual "pt"

        noCode.Length |> should equal 1
        noCode.Head |> should not' (be Null)
        noCode.Head.Value |> should equal title1

    [<Test>]
    let ``can define service title for legacy service`` () =
        let title1 = "Ilma keeleta palun"
        let title2 = "Portugalikeelne loba ..."

        let definition = ProducerDefinition(XRoadProtocol.Version20, "test", 1u)
        definition.Title.Add("", title1)
        definition.Title.Add("pt", title2)

        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xtee

        let titleElements = port.Elements(xtee "title") |> List.ofSeq
        titleElements.Length |> should equal 2

        let yesCode, noCode = titleElements |> List.partition (fun x -> x.Attributes().Any())

        yesCode.Length |> should equal 1
        yesCode.Head |> should not' (be Null)
        yesCode.Head.Value |> should equal title2
        yesCode.Head.Attribute(xml "lang") |> attributeValueShouldEqual "pt"

        noCode.Length |> should equal 1
        noCode.Head |> should not' (be Null)
        noCode.Head.Value |> should equal title1

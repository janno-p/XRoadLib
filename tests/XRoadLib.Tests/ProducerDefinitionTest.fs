namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Linq
open System.Xml.Linq
open XRoadLib
open XRoadLib.Protocols.Description

[<TestFixture>]
module ProducerDefinitionTest =
    let contractAssembly = typeof<XRoadLib.Tests.Contract.Class1>.Assembly

    let xn nm = XName.Get(nm)
    let wsdl nm = XName.Get(nm, NamespaceConstants.WSDL)
    let soap nm = XName.Get(nm, NamespaceConstants.SOAP)
    let xrd nm = XName.Get(nm, NamespaceConstants.XROAD)
    let xtee nm = XName.Get(nm, NamespaceConstants.XTEE)
    let xml nm = XName.Get(nm, NamespaceConstants.XML)
    let xsd nm = XName.Get(nm, NamespaceConstants.XSD)

    let attributeValueShouldEqual value (a: XAttribute) =
        a |> should not' (be Null)
        a.Value |> should equal value

    let shouldMatchInCommonParts (doc: XContainer) (xrdns: string -> XName) =
        let root = doc.Elements(wsdl "definitions").SingleOrDefault()
        root |> should not' (be Null)

        let service = root.Elements(wsdl "service").SingleOrDefault()
        service |> should not' (be Null)
        service.Attributes().Count() |> should equal 1
        service.Attribute(xn "name") |> attributeValueShouldEqual "TestService"
        service.Elements().Count() |> should equal 1

        let port = service.Elements(wsdl "port").SingleOrDefault()
        port |> should not' (be Null)
        port.Attributes().Count() |> should equal 2
        port.Attribute(xn "name") |> attributeValueShouldEqual "TestPort"
        port.Attribute(xn "binding") |> attributeValueShouldEqual "TestBinding"

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
        let doc = ProducerDefinition(Globals.XRoadProtocol31, contractAssembly, Nullable(1u)) |> getDocument
        let port = shouldMatchInCommonParts doc xrd

        let address = port.Elements(xrd "address").SingleOrDefault()
        address |> should not' (be Null)
        address.IsEmpty |> should equal true
        address.Attributes().Count() |> should equal 1
        address.Attribute(xn "producer") |> attributeValueShouldEqual "test-producer"

        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual "http://TURVASERVER/cgi-bin/consumer_proxy"

    [<Test>]
    let ``empty legacy format service description`` () =
        let doc = ProducerDefinition(Globals.XRoadProtocol20, contractAssembly, Nullable(1u)) |> getDocument
        let port = shouldMatchInCommonParts doc xtee

        let address = port.Elements(xtee "address").SingleOrDefault()
        address |> should not' (be Null)
        address.IsEmpty |> should equal true
        address.Attributes().Count() |> should equal 1
        address.Attribute(xn "producer") |> attributeValueShouldEqual "test-producer"

        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual "http://TURVASERVER/cgi-bin/consumer_proxy"

    [<Test>]
    let ``should define service location if given`` () =
        let url = "http://TURVASERVER/cgi-bin/consumer_proxy"
        let definition = ProducerDefinition(Globals.XRoadProtocol31, contractAssembly, Nullable(1u))
        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xrd
        port.Elements(soap "address").Single().Attribute(xn "location") |> attributeValueShouldEqual url

    [<Test>]
    let ``should define service title`` () =
        let definition = ProducerDefinition(Globals.XRoadProtocol31, contractAssembly, Nullable(1u))

        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xrd

        let titleElements = port.Elements(xrd "title") |> List.ofSeq
        titleElements.Length |> should equal 4

        let yesCode, noCode = titleElements |> List.partition (fun x -> x.Attributes().Any())

        yesCode.Length |> should equal 3
        match yesCode with
        | [ en; et; pt ] ->
            yesCode |> List.iter (fun x -> x |> should not' (be Null))
            en.Value |> should equal "XRoadLib test producer"
            en.Attribute(xml "lang") |> attributeValueShouldEqual "en"
            et.Value |> should equal "XRoadLib test andmekogu"
            et.Attribute(xml "lang") |> attributeValueShouldEqual "et"
            pt.Value |> should equal "Portugalikeelne loba ..."
            pt.Attribute(xml "lang") |> attributeValueShouldEqual "pt"
        | _ -> failwith "never"

        noCode.Length |> should equal 1
        noCode.Head |> should not' (be Null)
        noCode.Head.Value |> should equal "Ilma keeleta palun"

    [<Test>]
    let ``can define service title for legacy service`` () =
        let definition = ProducerDefinition(Globals.XRoadProtocol20, contractAssembly, Nullable(1u))

        let doc = definition |> getDocument
        let port = shouldMatchInCommonParts doc xtee

        let titleElements = port.Elements(xtee "title") |> List.ofSeq
        titleElements.Length |> should equal 4

        let yesCode, noCode = titleElements |> List.partition (fun x -> x.Attributes().Any())

        yesCode.Length |> should equal 3
        match yesCode with
        | [ en; et; pt ] ->
            yesCode |> List.iter (fun x -> x |> should not' (be Null))
            en.Value |> should equal "XRoadLib test producer"
            en.Attribute(xml "lang") |> attributeValueShouldEqual "en"
            et.Value |> should equal "XRoadLib test andmekogu"
            et.Attribute(xml "lang") |> attributeValueShouldEqual "et"
            pt.Value |> should equal "Portugalikeelne loba ..."
            pt.Attribute(xml "lang") |> attributeValueShouldEqual "pt"
        | _ -> failwith "never"

        noCode.Length |> should equal 1
        noCode.Head |> should not' (be Null)
        noCode.Head.Value |> should equal "Ilma keeleta palun"

    let [<Test>] ``Anonymous type should be nested under container type`` () =
        let doc = ProducerDefinition(Globals.XRoadProtocol31, contractAssembly, Nullable(1u)) |> getDocument
        let definitions = doc.Elements(wsdl "definitions") |> Seq.exactlyOne
        let types = definitions.Elements(wsdl "types") |> Seq.exactlyOne
        let schema = types.Elements(xsd "schema") |> Seq.exactlyOne
        schema.Elements(xsd "complexType") |> Seq.filter (fun e -> e.Attribute(xn "name").Value = "AnonymousType") |> Seq.isEmpty |> should be True
        let containerType = schema.Elements(xsd "complexType") |> Seq.filter (fun e -> e.Attribute(xn "name").Value = "ContainerType") |> Seq.exactlyOne
        let containerTypeParticle = containerType.Elements() |> Seq.exactlyOne
        containerTypeParticle.Name |> should equal (xsd "sequence")
        containerTypeParticle.Elements().Count() |> should equal 2
        let knownProperty = containerTypeParticle.Elements(xsd "element") |> Seq.filter (fun e -> e.Attribute(xn "name").Value = "KnownProperty") |> Seq.exactlyOne
        knownProperty.Attribute(xn "type").Value |> should equal "xsd:string"
        let anonymousProperty = containerTypeParticle.Elements(xsd "element") |> Seq.filter (fun e -> e.Attribute(xn "name").Value = "AnonymousProperty") |> Seq.exactlyOne
        anonymousProperty.Attribute(xn "type") |> should be Null
        let anonymousType = anonymousProperty.Elements() |> Seq.exactlyOne
        anonymousType.Name |> should equal (xsd "complexType")
        anonymousType.Attribute(xn "name") |> should be Null
        let anonymousSequence = anonymousType.Elements() |> Seq.exactlyOne
        anonymousSequence.Name |> should equal (xsd "sequence")
        anonymousSequence.Elements().Count() |> should equal 3
        anonymousSequence.Elements() |> Seq.zip [ "Property1"; "Property2"; "Property3"] |> Seq.iter (fun (name, el) -> el.Attribute(xn "name").Value |> should equal name)


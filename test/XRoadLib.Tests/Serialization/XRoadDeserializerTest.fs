namespace XRoadLib.Tests

open FsUnit
open NUnit.Framework
open System
open System.IO
open System.Text
open System.Xml
open XRoadLib
open XRoadLib.Extensions
open XRoadLib.Protocols.Headers
open XRoadLib.Serialization
open XRoadLib.Serialization.Template
open XRoadLib.Tests.Contract

[<TestFixture>]
module XRoadDeserializerTest =
    let [<Literal>] dtoVersion = 3u
    let serializerCache = Globals.XRoadProtocol31.GetSerializerCache(Nullable(dtoVersion))

    let serviceMap = serializerCache.GetServiceMap("Service1")

    let deserializeRequest templateXml contentXml =
        let template = XRoadXmlTemplate(templateXml, typeof<IService>.GetMethod("Service1"))
        use stream = new MemoryStream()
        use writer = new StreamWriter(stream)
        writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>")
        writer.WriteLine(@"<soapenv:Envelope xmlns:soapenv=""{0}"" soapenv:encodingStyle=""{1}"">", NamespaceConstants.SOAP_ENV, NamespaceConstants.SOAP_ENC)
        writer.WriteLine(@"<soapenv:Body>")
        writer.WriteLine(@"<tns:Service1 xmlns:tns=""{0}"">", Globals.XRoadProtocol20.ProducerNamespace)
        writer.WriteLine(contentXml: string)
        writer.WriteLine("@</tns:Service1>")
        writer.WriteLine("@</soapenv:Body>")
        writer.WriteLine("@</soapenv:Envelope>")
        writer.Flush()
        stream.Position <- 0L
        use reader = XmlReader.Create(stream)
        use messageReader = new XRoadMessageReader(stream, null, Encoding.UTF8, null, [Globals.XRoadProtocol20])
        use message = new XRoadMessage(Globals.XRoadProtocol20, XRoadHeader20(), XmlTemplate=template)
        messageReader.Read(message, false)
        reader.MoveToPayload(System.Xml.Linq.XName.Get("Service1", Globals.XRoadProtocol20.ProducerNamespace))
        serviceMap.DeserializeRequest(reader, message)

    [<Test>]
    let ``can handle optional parameters`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha>
                                <Param1>
                                    <Property1>123</Property1>
                                </Param1>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be False
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should be Null
        request.Param1.Property1 |> should equal 123

    [<Test>]
    let ``can handle array type`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                    <Property2>
                                        <Value1 />
                                    </Property2>
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha>
                                <Param1>
                                    <Property1>123</Property1>
                                    <Property2>
                                        <item><Value1>102715</Value1></item>
                                        <item><Value1>102716</Value1></item>
                                    </Property2>
                                </Param1>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be False
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should be Null
        request.Param1.Property1 |> should equal 123
        request.Param1.Property2 |> should not' (be Null)
        request.Param1.Property2.Length |> should equal 2
        request.Param1.Property2.[0] |> should not' (be Null)
        request.Param1.Property2.[0].Value1 |> should equal 102715L
        request.Param1.Property2.[1] |> should not' (be Null)
        request.Param1.Property2.[1].Value1 |> should equal 102716L

    [<Test>]
    let ``can handle empty array`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                    <Property2>
                                        <Value1 />
                                    </Property2>
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha>
                                <Param1>
                                    <Property1>123</Property1>
                                    <Property2 />
                                </Param1>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be False
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should be Null
        request.Param1.Property1 |> should equal 123
        request.Param1.Property2 |> should not' (be Null)
        request.Param1.Property2.Length |> should equal 0

    [<Test>]
    let ``can handle array null value`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                    <Property2>
                                        <Value1 />
                                    </Property2>
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                                <Param1>
                                    <Property1>123</Property1>
                                    <Property2 xsi:nil="1" />
                                </Param1>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request = unbox<Service1Request> inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be False
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should be Null
        request.Param1.Property1 |> should equal 123
        request.Param1.Property2 |> should be Null

    [<Test>]
    let ``can handle multiple simple properties`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                    <Property3 />
                                    <Property2>
                                        <Value1 />
                                    </Property2>
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                                <Param1>
                                    <Property1>123</Property1>
                                    <Property3>some value</Property3>
                                    <Property2>
                                        <item><Value1>102715</Value1></item>
                                        <item><Value1>102716</Value1></item>
                                    </Property2>
                                </Param1>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be False
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should be Null
        request.Param1.Property1 |> should equal 123
        request.Param1.Property3 |> should equal "some value"
        request.Param1.Property2 |> should not' (be Null)
        request.Param1.Property2.Length |> should equal 2
        request.Param1.Property2.[0] |> should not' (be Null)
        request.Param1.Property2.[0].Value1 |> should equal 102715L
        request.Param1.Property2.[1] |> should not' (be Null)
        request.Param1.Property2.[1].Value1 |> should equal 102716L

    [<Test>]
    let ``can handle multiple parameters`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                    <Property3 />
                                    <Property2>
                                        <Value1 />
                                    </Property2>
                                </Param1>
                                <Param2 />
                                <Param3>
                                    <Subject>
                                        <Name />
                                    </Subject>
                                </Param3>
                             </keha>"""

        let contentXml = """<keha xmlns:tns="http://test-producer.x-road.ee/producer/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                                <Param1>
                                    <Property1>123</Property1>
                                    <Property3>some value</Property3>
                                    <Property2>
                                        <item><Value1>102715</Value1></item>
                                        <item><Value1>102716</Value1></item>
                                    </Property2>
                                </Param1>
                                <Param3>
                                    <Subject xsi:type="tns:Person">
                                        <Name>Vello</Name>
                                    </Subject>
                                </Param3>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be True
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should not' (be Null)
        request.Param1.Property1 |> should equal 123
        request.Param1.Property3 |> should equal "some value"
        request.Param1.Property2 |> should not' (be Null)
        request.Param1.Property2.Length |> should equal 2
        request.Param1.Property2.[0] |> should not' (be Null)
        request.Param1.Property2.[0].Value1 |> should equal 102715L
        request.Param1.Property2.[1] |> should not' (be Null)
        request.Param1.Property2.[1].Value1 |> should equal 102716L
        request.Param3.Subject |> should not' (be Null)
        request.Param3.Subject |> should be instanceOfType<Person>
        request.Param3.Subject.Name |> should equal "Vello"

    [<Test>]
    let ``cannot deserialize abstract type`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1 />
                                <Param2 />
                                <Param3>
                                    <Subject />
                                </Param3>
                             </keha>"""

        let contentXml = """<keha>
                                <Param3>
                                    <Subject>
                                        <Name>Vello</Name>
                                    </Subject>
                                </Param3>
                            </keha>"""

        TestDelegate(fun _ -> deserializeRequest templateXml contentXml |> ignore)
        |> should (throwWithMessage "The type '{http://test-producer.x-road.ee/producer/}Subject' is abstract, type attribute is required to specify target type.") typeof<XRoadException>

    [<Test>]
    let ``can deserialize abstract type when null`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1 />
                                <Param2 />
                                <Param3>
                                    <Subject />
                                </Param3>
                             </keha>"""

        let contentXml = """<keha xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                                <Param3>
                                    <Subject xsi:nil="true" />
                                </Param3>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request = unbox<Service1Request> inputObject
        request.IsSpecified("Param1") |> should be False
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be True
        request.Param1 |> should be Null
        request.Param2 |> should be Null
        request.Param3 |> should not' (be Null)
        request.Param3.Subject |> should be Null

    [<Test>]
    let ``understands parameter type attribute`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <Property1 />
                                    <Property3 />
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha xmlns:tns="http://test-producer.x-road.ee/producer/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                                <Param1 xsi:type="tns:InheritsParamType1">
                                    <Property1>467</Property1>
                                    <Property3>hello</Property3>
                                </Param1>
                            </keha>"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("Param1") |> should be True
        request.IsSpecified("Param2") |> should be False
        request.IsSpecified("Param3") |> should be False
        request.Param1 |> should not' (be Null)
        request.Param2 |> should be Null
        request.Param3 |> should be Null
        request.Param1 |> should be instanceOfType<InheritsParamType1>

        let param1: InheritsParamType1 = unbox request.Param1
        param1.Property1 |> should equal 467
        param1.Property3 |> should equal "hello"

    [<Test>]
    let ``can handle empty content`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <param1 />
                                <param2 />
                                <param3 />
                             </keha>"""

        let contentXml = """<keha />"""

        let inputObject = deserializeRequest templateXml contentXml
        inputObject |> should be instanceOfType<Service1Request>

        let request: Service1Request = unbox inputObject
        request.IsSpecified("param1") |> should be False
        request.IsSpecified("param2") |> should be False
        request.IsSpecified("param3") |> should be False
        request.Param1 |> should be Null
        request.Param2 |> should be Null
        request.Param3 |> should be Null

    [<Test>]
    let ``cannot deserialize message when MIME content is missing`` () =
        let templateXml = """<?xml version="1.0" encoding="utf8"?>
                             <keha>
                                <Param1>
                                    <MimeContent>
                                        <Value />
                                    </MimeContent>
                                </Param1>
                                <Param2 />
                                <Param3 />
                             </keha>"""

        let contentXml = """<keha xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:SOAP-ENC="http://schemas.xmlsoap.org/soap/encoding/">
                                <Param1>
                                    <MimeContent>
                                        <Value xsi:type="SOAP-ENC:base64" href="cid:KcGPT5EOP0BC0DXQ5wmEFA==" />
                                    </MimeContent>
                                </Param1>
                            </keha>"""

        TestDelegate(fun _ -> deserializeRequest templateXml contentXml |> ignore)
        |> should (throwWithMessage "MIME multipart message does not contain content with ID `cid:KcGPT5EOP0BC0DXQ5wmEFA==`.") typeof<XRoadException>

    let [<Test>] ``can deserialize anonymous type`` () =
        use stream = new MemoryStream()
        use writer = new StreamWriter(stream, Encoding.UTF8)
        writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>")
        writer.WriteLine(@"<entity xsi:type=""tns:ContainerType"" xmlns:xsi=""{0}"" xmlns:tns=""{1}"">", NamespaceConstants.XSI, Globals.XRoadProtocol20.ProducerNamespace)
        writer.WriteLine(@"<AnonymousProperty>")
        writer.WriteLine(@"<Property1 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">1</Property1>", NamespaceConstants.XSD)
        writer.WriteLine(@"<Property2 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">2</Property2>", NamespaceConstants.XSD)
        writer.WriteLine(@"<Property3 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">3</Property3>", NamespaceConstants.XSD)
        writer.WriteLine(@"</AnonymousProperty>")
        writer.WriteLine(@"<KnownProperty xsi:type=""xsd:string"" xmlns:xsd=""{0}"">value</KnownProperty>", NamespaceConstants.XSD)
        writer.WriteLine(@"</entity>")
        writer.Flush()
        stream.Position <- 0L
        use reader = XmlReader.Create(stream)
        reader.MoveToElement(0) |> ignore
        let typeMap = serializerCache.GetTypeMap(typeof<Wsdl.ContainerType>)
        use message = new XRoadMessage()
        let entity = typeMap.Deserialize(reader, XRoadXmlTemplate.EmptyNode, Globals.TestDefinition(typeof<Wsdl.ContainerType>), message)
        entity |> should not' (be Null)
        entity |> should be instanceOfType<Wsdl.ContainerType>
        let container = unbox<Wsdl.ContainerType> entity
        container.KnownProperty |> should equal "value"
        container.AnonymousProperty |> should not' (be Null)
        container.AnonymousProperty.Property1 |> should equal "1"
        container.AnonymousProperty.Property2 |> should equal "2"
        container.AnonymousProperty.Property3 |> should equal "3"

    let [<Test>] ``anonymous type should not have explicit type`` () =
        use stream = new MemoryStream()
        use writer = new StreamWriter(stream, Encoding.UTF8)
        writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>")
        writer.WriteLine(@"<entity xsi:type=""tns:ContainerType"" xmlns:xsi=""{0}"" xmlns:tns=""{1}"">", NamespaceConstants.XSI, Globals.XRoadProtocol20.ProducerNamespace)
        writer.WriteLine(@"<AnonymousProperty xsi:type=""Test"">")
        writer.WriteLine(@"<Property1 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">1</Property1>", NamespaceConstants.XSD)
        writer.WriteLine(@"<Property2 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">2</Property2>", NamespaceConstants.XSD)
        writer.WriteLine(@"<Property3 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">3</Property3>", NamespaceConstants.XSD)
        writer.WriteLine(@"</AnonymousProperty>")
        writer.WriteLine(@"<KnownProperty xsi:type=""xsd:string"" xmlns:xsd=""{0}"">value</KnownProperty>", NamespaceConstants.XSD)
        writer.WriteLine(@"</entity>")
        writer.Flush()
        stream.Position <- 0L
        use reader = XmlReader.Create(stream)
        reader.MoveToElement(0) |> ignore
        let typeMap = serializerCache.GetTypeMap(typeof<Wsdl.ContainerType>)
        use message = new XRoadMessage()
        TestDelegate(fun _ -> typeMap.Deserialize(reader, XRoadXmlTemplate.EmptyNode, Globals.TestDefinition(typeof<Wsdl.ContainerType>), message) |> ignore)
        |> should (throwWithMessage "Expected anonymous type, but `Test` was given.") typeof<XRoadException>

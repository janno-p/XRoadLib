using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Serialization.Template;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Wsdl;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadDeserializerTest
    {
        private const uint DTO_VERSION = 3;

        private static readonly ISerializer serializer20 = Globals.ServiceManager20.GetSerializer(DTO_VERSION);
        private static readonly ISerializer serializer31 = Globals.ServiceManager31.GetSerializer(DTO_VERSION);
        private static readonly IServiceMap serviceMap20 = serializer20.GetServiceMap("Service1");
        private static readonly IServiceMap serviceMap31 = serializer31.GetServiceMap("Service1");

        [Fact]
        public void CanHandleOptionalParameters()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<keha>"
                           + "  <Param1>"
                           + "    <Property1>123</Property1>"
                           + "  </Param1>"
                           + "</keha>";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.False(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
            Assert.Equal(123, request.Param1.Property1);
        }

        [Fact]
        public void CanHandleArrayType()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "    <Property2>"
                            + "      <Value1 />"
                            + "    </Property2>"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<keha>"
                           + "  <Param1>"
                           + "    <Property1>123</Property1>"
                           + "    <Property2>"
                           + "      <item><Value1>102715</Value1></item>"
                           + "      <item><Value1>102716</Value1></item>"
                           + "    </Property2>"
                           + "  </Param1>"
                           + "</keha>";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.False(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
            Assert.Equal(123, request.Param1.Property1);
            Assert.NotNull(request.Param1.Property2);
            Assert.Equal(2, request.Param1.Property2.Length);
            Assert.NotNull(request.Param1.Property2[0]);
            Assert.Equal(102715L, request.Param1.Property2[0].Value1);
            Assert.NotNull(request.Param1.Property2[1]);
            Assert.Equal(102716L, request.Param1.Property2[1].Value1);
        }

        [Fact]
        public void CanHandleEmptyArray()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "    <Property2>"
                            + "      <Value1 />"
                            + "    </Property2>"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<keha>"
                           + "  <Param1>"
                           + "    <Property1>123</Property1>"
                           + "    <Property2 />"
                           + "  </Param1>"
                           + "</keha>";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.False(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
            Assert.Equal(123, request.Param1.Property1);
            Assert.NotNull(request.Param1.Property2);
            Assert.Empty(request.Param1.Property2);
        }

        [Fact]
        public void CanHandleArrayNullValue()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "    <Property2>"
                            + "      <Value1 />"
                            + "    </Property2>"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<keha xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                           + "  <Param1>"
                           + "    <Property1>123</Property1>"
                           + "    <Property2 xsi:nil=\"1\" />"
                           + "  </Param1>"
                           + "</keha>";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.False(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
            Assert.Equal(123, request.Param1.Property1);
            Assert.Null(request.Param1.Property2);
        }

        [Fact]
        public void CanHandleMultipleSimpleProperties()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "    <Property3 />"
                            + "    <Property2>"
                            + "      <Value1 />"
                            + "    </Property2>"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<keha xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                           + "  <Param1>"
                           + "    <Property1>123</Property1>"
                           + "    <Property3>some value</Property3>"
                           + "    <Property2>"
                           + "      <item><Value1>102715</Value1></item>"
                           + "      <item><Value1>102716</Value1></item>"
                           + "    </Property2>"
                           + "  </Param1>"
                           + "</keha>";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.False(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
            Assert.Equal(123, request.Param1.Property1);
            Assert.Equal("some value", request.Param1.Property3);
            Assert.NotNull(request.Param1.Property2);
            Assert.Equal(2, request.Param1.Property2.Length);
            Assert.NotNull(request.Param1.Property2[0]);
            Assert.Equal(102715L, request.Param1.Property2[0].Value1);
            Assert.NotNull(request.Param1.Property2[1]);
            Assert.Equal(102716L, request.Param1.Property2[1].Value1);
        }

        [Fact]
        public void CanHandleMultipleParameters()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "    <Property3 />"
                            + "    <Property2>"
                            + "      <Value1 />"
                            + "    </Property2>"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3>"
                            + "    <Subject>"
                            + "      <Name />"
                            + "    </Subject>"
                            + "  </Param3>"
                            + "</keha>";

            var contentXml = "<request xmlns:tns=\"http://test-producer.x-road.ee/producer/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                           + "  <Param1>"
                           + "    <Property1>123</Property1>"
                           + "    <Property3>some value</Property3>"
                           + "    <Property2>"
                           + "      <item><Value1>102715</Value1></item>"
                           + "      <item><Value1>102716</Value1></item>"
                           + "    </Property2>"
                           + "  </Param1>"
                           + "  <Param3>"
                           + "    <Subject xsi:type=\"tns:Person\">"
                           + "      <Name>Vello</Name>"
                           + "    </Subject>"
                           + "  </Param3>"
                           + "</request>";

            var inputObject = DeserializeRequest31(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.True(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.NotNull(request.Param3);
            Assert.Equal(123, request.Param1.Property1);
            Assert.Equal("some value", request.Param1.Property3);
            Assert.NotNull(request.Param1.Property2);
            Assert.Equal(2, request.Param1.Property2.Length);
            Assert.NotNull(request.Param1.Property2[0]);
            Assert.Equal(102715L, request.Param1.Property2[0].Value1);
            Assert.NotNull(request.Param1.Property2[1]);
            Assert.Equal(102716L, request.Param1.Property2[1].Value1);
            Assert.NotNull(request.Param3.Subject);
            Assert.IsType<Person>(request.Param3.Subject);
            Assert.Equal("Vello", request.Param3.Subject.Name);
        }

        [Fact]
        public void CannotDeserializeAbstractType()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1 />"
                            + "  <Param2 />"
                            + "  <Param3>"
                            + "    <Subject />"
                            + "  </Param3>"
                            + "</keha>";

            var contentXml = "<keha>"
                           + "  <Param3>"
                           + "    <Subject>"
                           + "      <Name>Vello</Name>"
                           + "    </Subject>"
                           + "  </Param3>"
                           + "</keha>";

            var exception = Assert.Throws<XRoadException>(() => DeserializeRequest20(templateXml, contentXml));
            Assert.Equal("The type '{http://producers.test-producer.xtee.riik.ee/producer/test-producer}Subject' is abstract, type attribute is required to specify target type.", exception.Message);
        }

        [Fact]
        public void CanDeserializeAbstractTypeWhenNull()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1 />"
                            + "  <Param2 />"
                            + "  <Param3>"
                            + "    <Subject />"
                            + "  </Param3>"
                            + "</keha>";

            var contentXml = "<keha xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                           + "  <Param3>"
                           + "    <Subject xsi:nil=\"true\" />"
                           + "  </Param3>"
                           + "</keha>";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.False(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.True(request.IsSpecified("Param3"));
            Assert.Null(request.Param1);
            Assert.Null(request.Param2);
            Assert.NotNull(request.Param3);
            Assert.Null(request.Param3.Subject);
        }

        [Fact]
        public void UnderstandsParameterTypeAttribute()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <Property1 />"
                            + "    <Property3 />"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<request xmlns:tns=\"http://test-producer.x-road.ee/producer/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                           + "  <Param1 xsi:type=\"tns:InheritsParamType1\">"
                           + "    <Property1>467</Property1>"
                           + "    <Property3>hello</Property3>"
                           + "  </Param1>"
                           + "</request>";

            var inputObject = DeserializeRequest31(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.True(request.IsSpecified("Param1"));
            Assert.False(request.IsSpecified("Param2"));
            Assert.False(request.IsSpecified("Param3"));
            Assert.NotNull(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
            Assert.IsType<InheritsParamType1>(request.Param1);

            var param1 = (InheritsParamType1)request.Param1;
            Assert.Equal(467, param1.Property1);
            Assert.Equal("hello", param1.Property3);
        }

        [Fact]
        public void CanHandleEmptyContent()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <param1 />"
                            + "  <param2 />"
                            + "  <param3 />"
                            + "</keha>";

            var contentXml = "<keha />";

            var inputObject = DeserializeRequest20(templateXml, contentXml);
            Assert.IsType<Service1Request>(inputObject);

            var request = (Service1Request)inputObject;
            Assert.False(request.IsSpecified("param1"));
            Assert.False(request.IsSpecified("param2"));
            Assert.False(request.IsSpecified("param3"));
            Assert.Null(request.Param1);
            Assert.Null(request.Param2);
            Assert.Null(request.Param3);
        }

        [Fact]
        public void CannotDeserializeMessageWhenMIMEContentIsMissing()
        {
            var templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                            + "<keha>"
                            + "  <Param1>"
                            + "    <MimeContent>"
                            + "      <Value />"
                            + "    </MimeContent>"
                            + "  </Param1>"
                            + "  <Param2 />"
                            + "  <Param3 />"
                            + "</keha>";

            var contentXml = "<keha xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\">"
                           + "  <Param1>"
                           + "    <MimeContent>"
                           + "      <Value xsi:type=\"SOAP-ENC:base64\" href=\"cid:KcGPT5EOP0BC0DXQ5wmEFA==\" />"
                           + "    </MimeContent>"
                           + "  </Param1>"
                           + "</keha>";

            var exception = Assert.Throws<XRoadException>(() => DeserializeRequest20(templateXml, contentXml));
            Assert.Equal("MIME multipart message does not contain content with ID `cid:KcGPT5EOP0BC0DXQ5wmEFA==`.", exception.Message);
        }

        [Fact]
        public void CanDeserializeAnonymousType()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                writer.WriteLine(@"<entity xsi:type=""tns:ContainerType"" xmlns:xsi=""{0}"" xmlns:tns=""{1}"">", NamespaceConstants.XSI, Globals.ServiceManager20.ProducerNamespace);
                writer.WriteLine(@"<AnonymousProperty>");
                writer.WriteLine(@"<Property1 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">1</Property1>", NamespaceConstants.XSD);
                writer.WriteLine(@"<Property2 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">2</Property2>", NamespaceConstants.XSD);
                writer.WriteLine(@"<Property3 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">3</Property3>", NamespaceConstants.XSD);
                writer.WriteLine(@"</AnonymousProperty>");
                writer.WriteLine(@"<KnownProperty xsi:type=""xsd:string"" xmlns:xsd=""{0}"">value</KnownProperty>", NamespaceConstants.XSD);
                writer.WriteLine(@"</entity>");
                writer.Flush();

                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    Assert.True(reader.MoveToElement(0));

                    var typeMap = serializer31.GetTypeMap(typeof(ContainerType));
                    using (var message = new XRoadMessage())
                    {
                        var entity = typeMap.Deserialize(reader, XRoadXmlTemplate.EmptyNode, Globals.GetTestDefinition(typeof(ContainerType)), message);
                        Assert.NotNull(entity);
                        Assert.IsType<ContainerType>(entity);

                        var container = (ContainerType)entity;
                        Assert.Equal("value", container.KnownProperty);
                        Assert.NotNull(container.AnonymousProperty);
                        Assert.Equal("1", container.AnonymousProperty.Property1);
                        Assert.Equal("2", container.AnonymousProperty.Property2);
                        Assert.Equal("3", container.AnonymousProperty.Property3);
                    }
                }
            }
        }

        [Fact]
        public void AnonymousTypeShouldNotHaveExplicitType()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                writer.WriteLine(@"<entity xsi:type=""tns:ContainerType"" xmlns:xsi=""{0}"" xmlns:tns=""{1}"">", NamespaceConstants.XSI, Globals.ServiceManager20.ProducerNamespace);
                writer.WriteLine(@"<AnonymousProperty xsi:type=""Test"">");
                writer.WriteLine(@"<Property1 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">1</Property1>", NamespaceConstants.XSD);
                writer.WriteLine(@"<Property2 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">2</Property2>", NamespaceConstants.XSD);
                writer.WriteLine(@"<Property3 xsi:type=""xsd:string"" xmlns:xsd=""{0}"">3</Property3>", NamespaceConstants.XSD);
                writer.WriteLine(@"</AnonymousProperty>");
                writer.WriteLine(@"<KnownProperty xsi:type=""xsd:string"" xmlns:xsd=""{0}"">value</KnownProperty>", NamespaceConstants.XSD);
                writer.WriteLine(@"</entity>");
                writer.Flush();

                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                {
                    Assert.True(reader.MoveToElement(0));
                    var typeMap = serializer31.GetTypeMap(typeof(ContainerType));
                    using (var message = new XRoadMessage())
                    {
                        var exception = Assert.Throws<XRoadException>(() => typeMap.Deserialize(reader, XRoadXmlTemplate.EmptyNode, Globals.GetTestDefinition(typeof(ContainerType)), message));
                        Assert.Equal("Expected anonymous type, but `Test` was given.", exception.Message);
                    }
                }
            }
        }

        private object DeserializeRequest20(string templateXml, string contentXml)
        {
            var template = new XRoadXmlTemplate(templateXml, typeof(IService).GetTypeInfo().GetMethod("Service1"));
            return DeserializeRequest(templateXml, contentXml, Globals.ServiceManager20, (msgr, xmlr) =>
            {
                var message = Globals.ServiceManager20.CreateMessage();
                message.XmlTemplate = template;

                using (message)
                {
                    msgr.Read(message, false);
                    xmlr.MoveToPayload(System.Xml.Linq.XName.Get("Service1", Globals.ServiceManager20.ProducerNamespace));
                    return serviceMap20.DeserializeRequest(xmlr, message);
                }
            });
        }

        private object DeserializeRequest31(string templateXml, string contentXml)
        {
            var template = new XRoadXmlTemplate(templateXml, typeof(IService).GetTypeInfo().GetMethod("Service1"));
            return DeserializeRequest(templateXml, contentXml, Globals.ServiceManager31, (msgr, xmlr) =>
            {
                var message = Globals.ServiceManager31.CreateMessage();
                message.XmlTemplate = template;

                using (message)
                {
                    msgr.Read(message, false);
                    xmlr.MoveToPayload(System.Xml.Linq.XName.Get("Service1", Globals.ServiceManager31.ProducerNamespace));
                    return serviceMap31.DeserializeRequest(xmlr, message);
                }
            });
        }

        private object DeserializeRequest(string templateXml, string contentXml, IServiceManager protocol, Func<XRoadMessageReader, XmlReader, object> deserializeMessage)
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                writer.WriteLine($"<soapenv:Envelope xmlns:soapenv=\"{NamespaceConstants.SOAP_ENV}\" soapenv:encodingStyle=\"{NamespaceConstants.SOAP_ENC}\">");
                writer.WriteLine(@"<soapenv:Body>");
                writer.WriteLine($"<tns:Service1 xmlns:tns=\"{protocol.ProducerNamespace}\">");
                writer.WriteLine(contentXml);
                writer.WriteLine("@</tns:Service1>");
                writer.WriteLine("@</soapenv:Body>");
                writer.WriteLine("@</soapenv:Envelope>");
                writer.Flush();

                stream.Position = 0;
                using (var reader = XmlReader.Create(stream))
                using (var messageReader = new XRoadMessageReader(stream, "text/xml; charset=UTF-8", null, new[] { protocol }))
                return deserializeMessage(messageReader, reader);
            }
        }
    }
}
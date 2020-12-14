using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Headers;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Serialization.Mapping;
using XRoadLib.Serialization.Template;
using XRoadLib.Soap;
using XRoadLib.Styles;
using XRoadLib.Tests.Contract;
using XRoadLib.Tests.Contract.Wsdl;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class XRoadDeserializerTest
    {
        private const uint DtoVersion = 3;

        private static readonly ISerializer Serializer = Globals.ServiceManager.GetSerializer(DtoVersion);
        private static readonly IServiceMap ServiceMap = Serializer.GetServiceMap("Service1");
        private static readonly IServiceMap Service3Map = Serializer.GetServiceMap("Service3");

        private static readonly IMessageFormatter MessageFormatter = new SoapMessageFormatter();

        [Fact]
        public async Task CanHandleOptionalParameters()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <Property1 />"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request>"
                                      + "  <Param1>"
                                      + "    <Property1>123</Property1>"
                                      + "  </Param1>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CanHandleArrayType()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <Property1 />"
                                       + "    <Property2>"
                                       + "      <Value1 />"
                                       + "    </Property2>"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request>"
                                      + "  <Param1>"
                                      + "    <Property1>123</Property1>"
                                      + "    <Property2>"
                                      + "      <item><Value1>102715</Value1></item>"
                                      + "      <item><Value1>102716</Value1></item>"
                                      + "    </Property2>"
                                      + "  </Param1>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CanHandleEmptyArray()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <Property1 />"
                                       + "    <Property2>"
                                       + "      <Value1 />"
                                       + "    </Property2>"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request>"
                                      + "  <Param1>"
                                      + "    <Property1>123</Property1>"
                                      + "    <Property2 />"
                                      + "  </Param1>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CanHandleArrayNullValue()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <Property1 />"
                                       + "    <Property2>"
                                       + "      <Value1 />"
                                       + "    </Property2>"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                                      + "  <Param1>"
                                      + "    <Property1>123</Property1>"
                                      + "    <Property2 xsi:nil=\"1\" />"
                                      + "  </Param1>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CanHandleMultipleSimpleProperties()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <Property1 />"
                                       + "    <Property3 />"
                                       + "    <Property2>"
                                       + "      <Value1 />"
                                       + "    </Property2>"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                                      + "  <Param1>"
                                      + "    <Property1>123</Property1>"
                                      + "    <Property2>"
                                      + "      <item><Value1>102715</Value1></item>"
                                      + "      <item><Value1>102716</Value1></item>"
                                      + "    </Property2>"
                                      + "    <Property3>some value</Property3>"
                                      + "  </Param1>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CanHandleMultipleParameters()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
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
                                       + "</request>";

            const string contentXml = "<request xmlns:tns=\"http://test-producer.x-road.eu/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                                      + "  <Param1>"
                                      + "    <Property1>123</Property1>"
                                      + "    <Property2>"
                                      + "      <item><Value1>102715</Value1></item>"
                                      + "      <item><Value1>102716</Value1></item>"
                                      + "    </Property2>"
                                      + "    <Property3>some value</Property3>"
                                      + "  </Param1>"
                                      + "  <Param3>"
                                      + "    <Subject xsi:type=\"tns:Person\">"
                                      + "      <Name>Vello</Name>"
                                      + "    </Subject>"
                                      + "  </Param3>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CannotDeserializeAbstractType()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1 />"
                                       + "  <Param2 />"
                                       + "  <Param3>"
                                       + "    <Subject />"
                                       + "  </Param3>"
                                       + "</request>";

            const string contentXml = "<request>"
                                      + "  <Param3>"
                                      + "    <Subject>"
                                      + "      <Name>Vello</Name>"
                                      + "    </Subject>"
                                      + "  </Param3>"
                                      + "</request>";

            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => DeserializeRequestAsync(templateXml, contentXml));
            Assert.Equal("The type '{http://test-producer.x-road.eu/}Subject' is abstract, type attribute is required to specify target type.", exception.Message);
        }

        [Fact]
        public async Task CanDeserializeAbstractTypeWhenNull()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1 />"
                                       + "  <Param2 />"
                                       + "  <Param3>"
                                       + "    <Subject />"
                                       + "  </Param3>"
                                       + "</request>";

            const string contentXml = "<request xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                                      + "  <Param3>"
                                      + "    <Subject xsi:nil=\"true\" />"
                                      + "  </Param3>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task UnderstandsParameterTypeAttribute()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <Property1 />"
                                       + "    <Property3 />"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request xmlns:tns=\"http://test-producer.x-road.eu/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                                      + "  <Param1 xsi:type=\"tns:InheritsParamType1\">"
                                      + "    <Property1>467</Property1>"
                                      + "    <Property3>hello</Property3>"
                                      + "  </Param1>"
                                      + "</request>";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CanHandleEmptyContent()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <param1 />"
                                       + "  <param2 />"
                                       + "  <param3 />"
                                       + "</request>";

            const string contentXml = "<request />";

            var inputObject = await DeserializeRequestAsync(templateXml, contentXml);
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
        public async Task CannotDeserializeMessageWhenMimeContentIsMissing()
        {
            const string templateXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                       + "<request>"
                                       + "  <Param1>"
                                       + "    <MimeContent>"
                                       + "      <Value />"
                                       + "    </MimeContent>"
                                       + "  </Param1>"
                                       + "  <Param2 />"
                                       + "  <Param3 />"
                                       + "</request>";

            const string contentXml = "<request xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\">"
                                      + "  <Param1>"
                                      + "    <Property1>0</Property1>"
                                      + "    <MimeContent>"
                                      + "      <Value xsi:type=\"SOAP-ENC:base64\" href=\"cid:KcGPT5EOP0BC0DXQ5wmEFA==\" />"
                                      + "    </MimeContent>"
                                      + "  </Param1>"
                                      + "</request>";

            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => DeserializeRequestAsync(templateXml, contentXml));
            Assert.Equal("MIME multipart message does not contain message part with ID `cid:KcGPT5EOP0BC0DXQ5wmEFA==`.", exception.Message);
        }

        [Fact]
        public async Task CanDeserializeAnonymousType()
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, XRoadEncoding.Utf8);

            await writer.WriteLineAsync(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            await writer.WriteLineAsync($@"<entity xsi:type=""tns:ContainerType"" xmlns:xsi=""{NamespaceConstants.Xsi}"" xmlns:tns=""{Globals.ServiceManager.ProducerNamespace}"">");
            await writer.WriteLineAsync(@"<AnonymousProperty>");
            await writer.WriteLineAsync($@"<Property1 xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">1</Property1>");
            await writer.WriteLineAsync($@"<Property2 xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">2</Property2>");
            await writer.WriteLineAsync($@"<Property3 xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">3</Property3>");
            await writer.WriteLineAsync(@"</AnonymousProperty>");
            await writer.WriteLineAsync($@"<KnownProperty xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">value</KnownProperty>");
            await writer.WriteLineAsync(@"</entity>");
            await writer.FlushAsync();

            stream.Position = 0;
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            Assert.True(await reader.MoveToElementAsync(0));

            var typeMap = Serializer.GetTypeMap(typeof(ContainerType));

            using var message = new XRoadMessage();
            var entity = await typeMap.DeserializeAsync(reader, XRoadXmlTemplate.EmptyNode, Globals.GetTestDefinition(typeof(ContainerType)), message);
            Assert.NotNull(entity);
            Assert.IsType<ContainerType>(entity);

            var container = (ContainerType)entity;
            Assert.Equal("value", container.KnownProperty);
            Assert.NotNull(container.AnonymousProperty);
            Assert.Equal("1", container.AnonymousProperty.Property1);
            Assert.Equal("2", container.AnonymousProperty.Property2);
            Assert.Equal("3", container.AnonymousProperty.Property3);
        }

        [Fact]
        public async Task AnonymousTypeShouldNotHaveExplicitType()
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, XRoadEncoding.Utf8);

            await writer.WriteLineAsync(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            await writer.WriteLineAsync($@"<entity xsi:type=""tns:ContainerType"" xmlns:xsi=""{NamespaceConstants.Xsi}"" xmlns:tns=""{Globals.ServiceManager.ProducerNamespace}"">");
            await writer.WriteLineAsync(@"<AnonymousProperty xsi:type=""Test"">");
            await writer.WriteLineAsync($@"<Property1 xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">1</Property1>");
            await writer.WriteLineAsync($@"<Property2 xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">2</Property2>");
            await writer.WriteLineAsync($@"<Property3 xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">3</Property3>");
            await writer.WriteLineAsync(@"</AnonymousProperty>");
            await writer.WriteLineAsync($@"<KnownProperty xsi:type=""xsd:string"" xmlns:xsd=""{NamespaceConstants.Xsd}"">value</KnownProperty>");
            await writer.WriteLineAsync(@"</entity>");
            await writer.FlushAsync();

            var anonymousProperty = typeof(ContainerType).GetProperty(nameof(ContainerType.AnonymousProperty));

            stream.Position = 0;
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            Assert.True(await reader.MoveToElementAsync(0));
            var typeMap = Serializer.GetTypeMap(typeof(ContainerType));

            using var message = new XRoadMessage();

            var exception = await Assert.ThrowsAsync<UnknownTypeException>(() => typeMap.DeserializeAsync(reader, XRoadXmlTemplate.EmptyNode, Globals.GetTestDefinition(typeof(ContainerType)), message));
            Assert.Equal("Expected anonymous type, but `Test` was given.", exception.Message);
            Assert.True(exception.TypeDefinition.IsAnonymous);
            Assert.Same(exception.TypeDefinition.Type, anonymousProperty?.PropertyType);
            Assert.IsType<PropertyDefinition>(exception.ParticleDefinition);
            Assert.Same(((PropertyDefinition)exception.ParticleDefinition).PropertyInfo, anonymousProperty);
            Assert.Equal(exception.QualifiedName, XName.Get("Test"));
        }

        [Fact]
        public async Task CanDeserializedMergedArrayWithEmptyContent()
        {
            var contentXml = "<request xmlns:tns=\"http://test-producer.x-road.ee/producer/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
                             + "  <Value />"
                             + "  <Code />"
                             + "  <Code />"
                             + "  <Code />"
                             + "  <Value2>Joy</Value2>"
                             + "</request>";

            var inputObject = await DeserializeRequestAsync(null, contentXml, Service3Map, "Service3");
            Assert.IsType<TestMergedArrayContent>(inputObject);

            var request = (TestMergedArrayContent)inputObject;
            Assert.True(request.IsSpecified("Value"));
            Assert.True(request.IsSpecified("Codes"));
            Assert.True(request.IsSpecified("Value2"));

            Assert.NotNull(request.Value);
            Assert.Equal("", request.Value);

            Assert.NotNull(request.Codes);
            Assert.Equal(3, request.Codes.Length);
            Assert.Equal(request.Codes, new[] { "", "", "" });

            Assert.NotNull(request.Value2);
            Assert.Equal("Joy", request.Value2);
        }

        private static Task<object> DeserializeRequestAsync(string templateXml, string contentXml, IServiceMap serviceMap = null, string serviceName = "Service1")
        {
            serviceMap ??= ServiceMap;
            var template = string.IsNullOrEmpty(templateXml) ? null : new XRoadXmlTemplate(templateXml, typeof(Service1Request));
            return DeserializeRequestContentAsync(contentXml, Globals.ServiceManager, serviceName, async msgr =>
            {
                var message = Globals.ServiceManager.CreateMessage();
                message.XmlTemplate = template;

                using var _ = message;

                await msgr.ReadAsync(message);

                message.ContentStream.Position = 0;
                using var reader = XmlReader.Create(message.ContentStream, new XmlReaderSettings { Async = true });

                await MessageFormatter.MoveToPayloadAsync(reader, XName.Get(serviceName, Globals.ServiceManager.ProducerNamespace));
                return await serviceMap.DeserializeRequestAsync(reader, message);
            });
        }

        private static async Task<object> DeserializeRequestContentAsync(string contentXml, IServiceManager protocol, string serviceName, Func<XRoadMessageReader, Task<object>> deserializeMessage)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);

            await writer.WriteLineAsync(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            await writer.WriteLineAsync($"<soapenv:Envelope xmlns:soapenv=\"{NamespaceConstants.SoapEnv}\" soapenv:encodingStyle=\"{NamespaceConstants.SoapEnc}\">");

            using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings { Async = true, ConformanceLevel = ConformanceLevel.Fragment, Encoding = XRoadEncoding.Utf8 }))
            {
                await xmlWriter.WriteStartElementAsync("soapenv", "Header", NamespaceConstants.SoapEnv);
                await new XRoadHeader
                {
                    Client = new XRoadClientIdentifier(),
                    Id = Guid.NewGuid().ToString(),
                    ProtocolVersion = "4.0"
                }.WriteToAsync(xmlWriter, new DocLiteralStyle(), new HeaderDefinition<XRoadHeader>("RequiredHeaders"));
                await xmlWriter.WriteEndElementAsync();
            }

            await writer.WriteLineAsync(@"<soapenv:Body>");
            await writer.WriteLineAsync($"<tns:{serviceName} xmlns:tns=\"{protocol.ProducerNamespace}\">");
            await writer.WriteLineAsync(contentXml);
            await writer.WriteLineAsync($"@</tns:{serviceName}>");
            await writer.WriteLineAsync("@</soapenv:Body>");
            await writer.WriteLineAsync("@</soapenv:Envelope>");
            await writer.FlushAsync();

            stream.Position = 0;

            using var messageReader = new XRoadMessageReader(new DataReader(stream), MessageFormatter, "text/xml; charset=UTF-8", null, new[] { protocol });

            return await deserializeMessage(messageReader);
        }
    }
}
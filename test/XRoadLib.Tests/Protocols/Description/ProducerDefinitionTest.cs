using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests.Protocols.Description
{
    public class ProducerDefinitionTest
    {
        private static readonly Func<string, XName> Wsdl = x => XName.Get(x, NamespaceConstants.Wsdl);
        private static readonly Func<string, XName> Soap = x => XName.Get(x, NamespaceConstants.Soap);
        private static readonly Func<string, XName> XRoad = x => XName.Get(x, NamespaceConstants.XRoad);
        private static readonly Func<string, XName> Xsd = x => XName.Get(x, NamespaceConstants.Xsd);

        [Fact]
        public async Task ShouldDefineServiceLocationIfGiven()
        {
            const string url = "http://INSERT_CORRECT_SERVICE_URL";
            var doc = await GetDocumentAsync(Globals.ServiceManager, 1);
            var port = GetPort(doc, XRoad);
            Assert.Equal(url, port.Elements(Soap("address")).Single().Attribute("location")?.Value);
        }

        [Fact]
        public async Task AnonymousTypeShouldBeNestedUnderContainerType()
        {
            var doc = await GetDocumentAsync(Globals.ServiceManager, 1u);
            var definitions = doc.Elements(Wsdl("definitions")).Single();
            var types = definitions.Elements(Wsdl("types")).Single();
            var schema = types.Elements(Xsd("schema")).Single(x => x.Attribute("targetNamespace")?.Value == Globals.ServiceManager.ProducerNamespace);
            Assert.DoesNotContain(schema.Elements(Xsd("complexType")), e => e.Attribute("name")?.Value == "AnonymousType");

            var containerType = schema.Elements(Xsd("complexType")).Single(e => e.Attribute("name")?.Value == "ContainerType");
            var containerTypeParticle = containerType.Elements().Single();
            Assert.Equal(Xsd("sequence"), containerTypeParticle.Name);
            Assert.Equal(2, containerTypeParticle.Elements().Count());

            var knownProperty = containerTypeParticle.Elements(Xsd("element")).Single(e => e.Attribute("name")?.Value == "KnownProperty");
            Assert.Equal($"{knownProperty.GetPrefixOfNamespace(NamespaceConstants.Xsd)}:string", knownProperty.Attribute("type")?.Value);

            var anonymousProperty = containerTypeParticle.Elements(Xsd("element")).Single(e => e.Attribute("name")?.Value == "AnonymousProperty");
            Assert.Null(anonymousProperty.Attribute("type"));

            var anonymousType = anonymousProperty.Elements().Single();
            Assert.Equal(Xsd("complexType"), anonymousType.Name);
            Assert.Null(anonymousType.Attribute("name"));

            var anonymousSequence = anonymousType.Elements().Single();
            Assert.Equal(Xsd("sequence"), anonymousSequence.Name);
            Assert.Equal(3, anonymousSequence.Elements().Count());

            Assert.Collection(anonymousSequence.Elements(),
                              x => Assert.Equal("Property1", x.Attribute("name")?.Value),
                              x => Assert.Equal("Property2", x.Attribute("name")?.Value),
                              x => Assert.Equal("Property3", x.Attribute("name")?.Value));
        }

        private static XElement GetPort(XContainer doc, Func<string, XName> xrdns)
        {
            var root = doc.Elements(Wsdl("definitions")).SingleOrDefault();
            Assert.NotNull(root);

            var service = root.Elements(Wsdl("service")).SingleOrDefault();
            Assert.NotNull(service);
            Assert.Single(service.Attributes());
            Assert.Equal("ServiceName", service.Attribute("name")?.Value);
            Assert.Single(service.Elements());

            var port = service.Elements(Wsdl("port")).SingleOrDefault();
            Assert.NotNull(port);
            Assert.Equal(2, port.Attributes().Count());
            Assert.Equal("PortName", port.Attribute("name")?.Value);
            Assert.Equal("tns:BindingName", port.Attribute("binding")?.Value);

            Assert.DoesNotContain(port.Elements(), e => e.Name != Soap("address") && e.Name != xrdns("address") && e.Name != xrdns("title"));

            var soapAddress = port.Elements(Soap("address")).SingleOrDefault();
            Assert.NotNull(soapAddress);
            Assert.True(soapAddress.IsEmpty);
            Assert.Single(soapAddress.Attributes());

            return port;
        }

        private static async Task<XDocument> GetDocumentAsync(IServiceManager serviceManager, uint version)
        {
            using var stream = new MemoryStream();
            using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Encoding = XRoadEncoding.Utf8 });
            await serviceManager.WriteServiceDefinitionAsync(writer, version: version);
            stream.Position = 0;
            return XDocument.Load(stream);
        }
    }
}
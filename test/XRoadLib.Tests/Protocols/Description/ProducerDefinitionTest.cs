using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using XRoadLib.Tests.Contract;
using Xunit;

namespace XRoadLib.Tests.Protocols.Description
{
    public class ProducerDefinitionTest
    {
        private static readonly Assembly contractAssembly = typeof(Class1).GetTypeInfo().Assembly;

        private static readonly Func<string, XName> wsdl = x => XName.Get(x, NamespaceConstants.WSDL);
        private static readonly Func<string, XName> soap = x => XName.Get(x, NamespaceConstants.SOAP);
        private static readonly Func<string, XName> xroad = x => XName.Get(x, NamespaceConstants.XROAD);
        private static readonly Func<string, XName> xtee = x => XName.Get(x, NamespaceConstants.XTEE);
        private static readonly Func<string, XName> xml = x => XName.Get(x, NamespaceConstants.XML);
        private static readonly Func<string, XName> xsd = x => XName.Get(x, NamespaceConstants.XSD);

        [Fact]
        public void EmptyServiceDescription()
        {
            var doc = GetDocument(Globals.XRoadProtocol31, 1);
            var port = GetPort(doc, xroad);

            var address = port.Elements(xroad("address")).SingleOrDefault();
            Assert.NotNull(address);
            Assert.True(address.IsEmpty);
            Assert.Equal(1, address.Attributes().Count());
            Assert.Equal("test-producer", address.Attribute("producer").Value);

            Assert.Equal("http://TURVASERVER/cgi-bin/consumer_proxy", port.Elements(soap("address")).Single().Attribute("location").Value);
        }

        [Fact]
        public void EmptyLegacyFormatServiceDescription()
        {
            var doc = GetDocument(Globals.XRoadProtocol20, 1);
            var port = GetPort(doc, xtee);

            var address = port.Elements(xtee("address")).SingleOrDefault();
            Assert.NotNull(address);
            Assert.True(address.IsEmpty);
            Assert.Equal(1, address.Attributes().Count());
            Assert.Equal("test-producer", address.Attribute("producer").Value);

            Assert.Equal("http://TURVASERVER/cgi-bin/consumer_proxy", port.Elements(soap("address")).Single().Attribute("location").Value);
        }

        [Fact]
        public void ShouldDefineServiceLocationIfGiven()
        {
            var url = "http://TURVASERVER/cgi-bin/consumer_proxy";
            var doc = GetDocument(Globals.XRoadProtocol31, 1);
            var port = GetPort(doc, xroad);
            Assert.Equal(url, port.Elements(soap("address")).Single().Attribute("location").Value);
        }

        [Fact]
        public void ShouldDefineServiceTitle()
        {
            var doc = GetDocument(Globals.XRoadProtocol31, 1);
            var port = GetPort(doc, xroad);

            var titleElements = port.Elements(xroad("title")).ToList();
            Assert.Equal(4, titleElements.Count);

            var groupedByAttributes = titleElements.GroupBy(x => x.Attributes().Any()).ToList();

            var yesCode = groupedByAttributes.Where(x => x.Key).SelectMany(x => x).ToList();
            Assert.Equal(3, yesCode.Count);
            Assert.Collection(yesCode,
                              x =>
                              {
                                  Assert.Equal("XRoadLib test producer", x.Value);
                                  Assert.Equal("en", x.Attribute(xml("lang")).Value);
                              },
                              x =>
                              {
                                  Assert.Equal("XRoadLib test andmekogu", x.Value);
                                  Assert.Equal("et", x.Attribute(xml("lang")).Value);
                              },
                              x =>
                              {
                                  Assert.Equal("Portugalikeelne loba ...", x.Value);
                                  Assert.Equal("pt", x.Attribute(xml("lang")).Value);
                              });

            var noCode = groupedByAttributes.Where(x => !x.Key).SelectMany(x => x).ToList();
            Assert.Equal(1, noCode.Count);
            Assert.Equal("Ilma keeleta palun", noCode[0].Value);
        }

        [Fact]
        public void CanDefineServiceTitleForLegacyService()
        {
            var doc = GetDocument(Globals.XRoadProtocol20, 1);
            var port = GetPort(doc, xtee);

            var titleElements = port.Elements(xtee("title")).ToList();
            Assert.Equal(4, titleElements.Count);

            var groupedByAttributes = titleElements.GroupBy(x => x.Attributes().Any());

            var yesCode = groupedByAttributes.Where(x => x.Key).SelectMany(x => x).ToList();
            Assert.Equal(3, yesCode.Count);
            Assert.Collection(yesCode,
                              x =>
                              {
                                  Assert.Equal("XRoadLib test producer", x.Value);
                                  Assert.Equal("en", x.Attribute(xml("lang")).Value);
                              },
                              x =>
                              {
                                  Assert.Equal("XRoadLib test andmekogu", x.Value);
                                  Assert.Equal("et", x.Attribute(xml("lang")).Value);
                              },
                              x =>
                              {
                                  Assert.Equal("Portugalikeelne loba ...", x.Value);
                                  Assert.Equal("pt", x.Attribute(xml("lang")).Value);
                              });

            var noCode = groupedByAttributes.Where(x => !x.Key).SelectMany(x => x).ToList();
            Assert.Equal(1, noCode.Count);
            Assert.Equal("Ilma keeleta palun", noCode[0].Value);
        }

        [Fact]
        public void AnonymousTypeShouldBeNestedUnderContainerType()
        {
            var doc = GetDocument(Globals.XRoadProtocol31, 1u);
            var definitions = doc.Elements(wsdl("definitions")).Single();
            var types = definitions.Elements(wsdl("types")).Single();
            var schema = types.Elements(xsd("schema")).Single();
            Assert.False(schema.Elements(xsd("complexType")).Any(e => e.Attribute("name").Value == "AnonymousType"));

            var containerType = schema.Elements(xsd("complexType")).Single(e => e.Attribute("name").Value == "ContainerType");
            var containerTypeParticle = containerType.Elements().Single();
            Assert.Equal(xsd("sequence"), containerTypeParticle.Name);
            Assert.Equal(2, containerTypeParticle.Elements().Count());

            var knownProperty = containerTypeParticle.Elements(xsd("element")).Single(e => e.Attribute("name").Value == "KnownProperty");
            Assert.Equal("xsd:string", knownProperty.Attribute("type").Value);

            var anonymousProperty = containerTypeParticle.Elements(xsd("element")).Single(e => e.Attribute("name").Value == "AnonymousProperty");
            Assert.Null(anonymousProperty.Attribute("type"));

            var anonymousType = anonymousProperty.Elements().Single();
            Assert.Equal(xsd("complexType"), anonymousType.Name);
            Assert.Null(anonymousType.Attribute("name"));

            var anonymousSequence = anonymousType.Elements().Single();
            Assert.Equal(xsd("sequence"), anonymousSequence.Name);
            Assert.Equal(3, anonymousSequence.Elements().Count());

            Assert.Collection(anonymousSequence.Elements(),
                              x => Assert.Equal("Property1", x.Attribute("name").Value),
                              x => Assert.Equal("Property2", x.Attribute("name").Value),
                              x => Assert.Equal("Property3", x.Attribute("name").Value));
        }

        private XElement GetPort(XContainer doc, Func<string, XName> xrdns)
        {
            var root = doc.Elements(wsdl("definitions")).SingleOrDefault();
            Assert.NotNull(root);

            var service = root.Elements(wsdl("service")).SingleOrDefault();
            Assert.NotNull(service);
            Assert.Equal(1, service.Attributes().Count());
            Assert.Equal("TestService", service.Attribute("name").Value);
            Assert.Equal(1, service.Elements().Count());

            var port = service.Elements(wsdl("port")).SingleOrDefault();
            Assert.NotNull(port);
            Assert.Equal(2, port.Attributes().Count());
            Assert.Equal("TestPort", port.Attribute("name").Value);
            Assert.Equal("TestBinding", port.Attribute("binding").Value);

            Assert.False(port.Elements().Any(e => e.Name != soap("address") && e.Name != xrdns("address") && e.Name != xrdns("title")));

            var soapAddress = port.Elements(soap("address")).SingleOrDefault();
            Assert.NotNull(soapAddress);
            Assert.True(soapAddress.IsEmpty);
            Assert.Equal(1, soapAddress.Attributes().Count());

            return port;
        }

        private XDocument GetDocument(IXRoadProtocol protocol, uint version)
        {
            using (var stream = new MemoryStream())
            {
                protocol.WriteServiceDescription(stream, version);
                stream.Position = 0;
                return XDocument.Load(stream);
            }
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XRoadLib.Extensions;
using Xunit;

namespace XRoadLib.Tests.Protocols.Description
{
    public class ProducerDefinitionTest
    {
        private static readonly Func<string, XName> Wsdl = x => XName.Get(x, NamespaceConstants.Wsdl);
        private static readonly Func<string, XName> Soap = x => XName.Get(x, NamespaceConstants.Soap);
        private static readonly Func<string, XName> Xroad = x => XName.Get(x, NamespaceConstants.XRoad);
        private static readonly Func<string, XName> Xtee = x => XName.Get(x, NamespaceConstants.Xtee);
        private static readonly Func<string, XName> Xml = x => XName.Get(x, NamespaceConstants.Xml);
        private static readonly Func<string, XName> Xsd = x => XName.Get(x, NamespaceConstants.Xsd);

        [Fact]
        public void EmptyServiceDescription()
        {
            var doc = GetDocument(Globals.ServiceManager31, 1);
            var port = GetPort(doc, Xroad);

            var address = port.Elements(Xroad("address")).SingleOrDefault();
            Assert.NotNull(address);
            Assert.True(address.IsEmpty);
            Assert.Single(address.Attributes());
            Assert.Equal("test-producer", address.Attribute("producer")?.Value);

            Assert.Equal("http://TURVASERVER/cgi-bin/consumer_proxy", port.Elements(Soap("address")).Single().Attribute("location")?.Value);
        }

        [Fact]
        public void EmptyLegacyFormatServiceDescription()
        {
            var doc = GetDocument(Globals.ServiceManager20, 1);
            var port = GetPort(doc, Xtee);

            var address = port.Elements(Xtee("address")).SingleOrDefault();
            Assert.NotNull(address);
            Assert.True(address.IsEmpty);
            Assert.Single(address.Attributes());
            Assert.Equal("test-producer", address.Attribute("producer")?.Value);

            Assert.Equal("http://TURVASERVER/cgi-bin/consumer_proxy", port.Elements(Soap("address")).Single().Attribute("location")?.Value);
        }

        [Fact]
        public void ShouldDefineServiceLocationIfGiven()
        {
            var url = "http://TURVASERVER/cgi-bin/consumer_proxy";
            var doc = GetDocument(Globals.ServiceManager31, 1);
            var port = GetPort(doc, Xroad);
            Assert.Equal(url, port.Elements(Soap("address")).Single().Attribute("location")?.Value);
        }

        [Fact]
        public void ShouldDefineServiceTitle()
        {
            var doc = GetDocument(Globals.ServiceManager31, 1);
            var port = GetPort(doc, Xroad);

            var titleElements = port.Elements(Xroad("title")).ToList();
            Assert.Equal(4, titleElements.Count);

            var groupedByAttributes = titleElements.GroupBy(x => x.Attributes().Any()).ToList();

            var yesCode = groupedByAttributes.Where(x => x.Key).SelectMany(x => x).ToList();
            Assert.Equal(3, yesCode.Count);
            Assert.Collection(yesCode,
                              x =>
                              {
                                  Assert.Equal("XRoadLib test producer", x.Value);
                                  Assert.Equal("en", x.Attribute(Xml("lang"))?.Value);
                              },
                              x =>
                              {
                                  Assert.Equal("XRoadLib test andmekogu", x.Value);
                                  Assert.Equal("et", x.Attribute(Xml("lang"))?.Value);
                              },
                              x =>
                              {
                                  Assert.Equal("Portugalikeelne loba ...", x.Value);
                                  Assert.Equal("pt", x.Attribute(Xml("lang"))?.Value);
                              });

            var noCode = groupedByAttributes.Where(x => !x.Key).SelectMany(x => x).ToList();
            Assert.Single(noCode);
            Assert.Equal("Ilma keeleta palun", noCode[0].Value);
        }

        [Fact]
        public void CanDefineServiceTitleForLegacyService()
        {
            var doc = GetDocument(Globals.ServiceManager20, 1);
            var port = GetPort(doc, Xtee);

            var titleElements = port.Elements(Xtee("title")).ToList();
            Assert.Equal(4, titleElements.Count);

            var groupedByAttributes = titleElements.GroupBy(x => x.Attributes().Any()).ToList();

            var yesCode = groupedByAttributes.Where(x => x.Key).SelectMany(x => x).ToList();
            Assert.Equal(3, yesCode.Count);
            Assert.Collection(yesCode,
                              x =>
                              {
                                  Assert.Equal("XRoadLib test producer", x.Value);
                                  Assert.Equal("en", x.Attribute(Xml("lang"))?.Value);
                              },
                              x =>
                              {
                                  Assert.Equal("XRoadLib test andmekogu", x.Value);
                                  Assert.Equal("et", x.Attribute(Xml("lang"))?.Value);
                              },
                              x =>
                              {
                                  Assert.Equal("Portugalikeelne loba ...", x.Value);
                                  Assert.Equal("pt", x.Attribute(Xml("lang"))?.Value);
                              });

            var noCode = groupedByAttributes.Where(x => !x.Key).SelectMany(x => x).ToList();
            Assert.Single(noCode);
            Assert.Equal("Ilma keeleta palun", noCode[0].Value);
        }

        [Fact]
        public void AnonymousTypeShouldBeNestedUnderContainerType()
        {
            var doc = GetDocument(Globals.ServiceManager31, 1u);
            var definitions = doc.Elements(Wsdl("definitions")).Single();
            var types = definitions.Elements(Wsdl("types")).Single();
            var schema = types.Elements(Xsd("schema")).Single(x => x.Attribute("targetNamespace")?.Value == Globals.ServiceManager31.ProducerNamespace);
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

        private XElement GetPort(XContainer doc, Func<string, XName> xrdns)
        {
            var root = doc.Elements(Wsdl("definitions")).SingleOrDefault();
            Assert.NotNull(root);

            var service = root.Elements(Wsdl("service")).SingleOrDefault();
            Assert.NotNull(service);
            Assert.Single(service.Attributes());
            Assert.Equal("TestService", service.Attribute("name")?.Value);
            Assert.Single(service.Elements());

            var port = service.Elements(Wsdl("port")).SingleOrDefault();
            Assert.NotNull(port);
            Assert.Equal(2, port.Attributes().Count());
            Assert.Equal("TestPort", port.Attribute("name")?.Value);
            Assert.Equal("tns:TestBinding", port.Attribute("binding")?.Value);

            Assert.DoesNotContain(port.Elements(), e => e.Name != Soap("address") && e.Name != xrdns("address") && e.Name != xrdns("title"));

            var soapAddress = port.Elements(Soap("address")).SingleOrDefault();
            Assert.NotNull(soapAddress);
            Assert.True(soapAddress.IsEmpty);
            Assert.Single(soapAddress.Attributes());

            return port;
        }

        private static XDocument GetDocument(IServiceManager serviceManager, uint version)
        {
            using var stream = new MemoryStream();
            serviceManager.CreateServiceDescription(version: version).SaveTo(stream);
            stream.Position = 0;
            return XDocument.Load(stream);
        }
    }
}
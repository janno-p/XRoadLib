using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XRoadLib.Headers;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class ParseXRoadHeader40Test
    {
        private static readonly Func<string, string> minimalValidHeader = x => $"<xrd:client id:objectType=\"MEMBER\"><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion>{x}";

        [Fact]
        public void NoHeader()
        {
            var tuple = ParseHeader("");
            Assert.Null(tuple.Item1);
            Assert.Equal(0, tuple.Item2.Count);
            Assert.Null(tuple.Item3);
        }

        [Fact]
        public void ValidatesPresenceOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader("<xrd:id>test</xrd:id>"));
            Assert.Equal("X-Road header `client` element is mandatory.", exception.Message);
        }

        [Fact]
        public void ValidatesContentOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader("<xrd:client />"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` cannot be empty.", exception.Message);
        }

        [Fact]
        public void ValidatesObjectTypeAttributeOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client></xrd:client>"));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.", exception.Message);
        }

        [Fact]
        public void ValidatesXRoadInstanceSubelementOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""></xrd:client>"));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public void ValidatesXRoadInstanceSubelementOfClientElementWithUnknownElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><x /></xrd:client>"));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public void ValidatesMemberClassSubelementOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /></xrd:client>"));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}memberClass`.", exception.Message);
        }

        [Fact]
        public void ValidatesMemberCodeSubelementOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /></xrd:client>"));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}memberCode`.", exception.Message);
        }

        [Fact]
        public void ValidatesInvalidSubelementOfClientElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /><x /></xrd:client>"));
        	Assert.Equal("Unexpected element `{http://schemas.xmlsoap.org/soap/envelope/}x` in element `{http://x-road.eu/xsd/xroad.xsd}client`.", exception.Message);
        }

        [Fact]
        public void ValidatesPresenceOfIdElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client>"));
        	Assert.Equal("X-Road header `id` element is mandatory.", exception.Message);
        }

        [Fact]
        public void ValidatesPresenceOfProtocolVersionElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id />"));
        	Assert.Equal("X-Road header `protocolVersion` element is mandatory.", exception.Message);
        }

        [Fact]
        public void ValidatesValueOfProtocolVersionElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id /><xrd:protocolVersion />"));
            Assert.Equal("Unsupported X-Road v6 protocol version value ``.", exception.Message);
        }

        [Fact]
        public void CollectsClientMandatoryValues()
        {
            var tuple = ParseHeader(minimalValidHeader(""));
            Assert.NotNull(tuple.Item1);
            Assert.Same(Globals.XRoadProtocol40, tuple.Item3);
            Assert.NotNull(tuple.Item1.Client);
            Assert.Equal("EE", tuple.Item1.Client.XRoadInstance);
            Assert.Equal("GOV", tuple.Item1.Client.MemberClass);
            Assert.Equal("12345", tuple.Item1.Client.MemberCode);
            Assert.Equal(XRoadObjectType.Member, tuple.Item1.Client.ObjectType);
            Assert.Null(tuple.Item1.Client.SubsystemCode);
            Assert.Equal("ABCDE", tuple.Item1.Id);
            Assert.Null(tuple.Item1.Issue);
            Assert.Equal("4.0", tuple.Item1.ProtocolVersion);
            Assert.Null(tuple.Item1.Service);
            Assert.Null(tuple.Item1.UserId);
        }

        [Fact]
        public void CollectsUnqualifiedSoapHeaderElementsButCannotDetectProtocolVersion()
        {
            var tuple = ParseHeader(@"<x><test>bla</test></x><y /><z />");
            Assert.Null(tuple.Item1);
            Assert.Null(tuple.Item3);
            Assert.Equal(3, tuple.Item2.Count);

            var elX = tuple.Item2.SingleOrDefault(x => x.Name.LocalName == "x");
            Assert.NotNull(elX);

            var elXTest = elX.Element(XName.Get("test", NamespaceConstants.SOAP_ENV));
            Assert.NotNull(elXTest);
            Assert.Equal("bla", elXTest.Value);
            Assert.True(tuple.Item2.Any(x => x.Name.LocalName == "y"));
            Assert.True(tuple.Item2.Any(x => x.Name.LocalName == "z"));
        }

        [Fact]
        public void CollectsUnqualifiedSoapHeadersThatAreMixedWithXRoadElements()
        {
            var tuple = ParseHeader(@"<x /><xrd:client id:objectType=""MEMBER""><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><y /><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion><z />");
            Assert.NotNull(tuple.Item1);
            Assert.Same(Globals.XRoadProtocol40, tuple.Item3);
            Assert.NotNull(tuple.Item1.Client);
            Assert.Equal("EE", tuple.Item1.Client.XRoadInstance);
            Assert.Equal("GOV", tuple.Item1.Client.MemberClass);
            Assert.Equal("12345", tuple.Item1.Client.MemberCode);
            Assert.Equal(XRoadObjectType.Member, tuple.Item1.Client.ObjectType);
            Assert.Null(tuple.Item1.Client.SubsystemCode);
            Assert.Equal("ABCDE", tuple.Item1.Id);
            Assert.Null(tuple.Item1.Issue);
            Assert.Equal("4.0", tuple.Item1.ProtocolVersion);
            Assert.Null(tuple.Item1.Service);
            Assert.Null(tuple.Item1.UserId);
            Assert.Equal(3, tuple.Item2.Count);
            Assert.True(tuple.Item2.Any(x => x.Name.LocalName == "x"));
            Assert.True(tuple.Item2.Any(x => x.Name.LocalName == "y"));
            Assert.True(tuple.Item2.Any(x => x.Name.LocalName == "z"));
        }

        [Fact]
        public void UnrecognizedXRoadHeaderElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<xrd:x />"));
            Assert.Equal("Unexpected X-Road header element `{http://x-road.eu/xsd/xroad.xsd}x`.", exception.Message);
        }

        [Fact]
        public void EmptyServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service />")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` cannot be empty.", exception.Message);
        }

        [Fact]
        public void MissingObjectTypeAttributeForServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service></xrd:service>")));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.", exception.Message);
        }

        [Fact]
        public void MissingXRoadInstanceElementForServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public void InvalidElementInServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><x /></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public void MissingMemberClassElementForServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /></xrd:service>")));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}memberClass`.", exception.Message);
        }

        [Fact]
        public void MissingMemberCodeElementForServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}memberCode`.", exception.Message);
        }

        [Fact]
        public void MissingServiceCodeElementForServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:service>")));
        	Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}serviceCode`.", exception.Message);
        }

        [Fact]
        public void ReadMinimalGroupOfElementsForServiceElement()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:serviceCode /></xrd:service>"));
            Assert.NotNull(tuple.Item1);
            Assert.NotNull(tuple.Item1.Service);
            Assert.Equal("", tuple.Item1.Service.XRoadInstance);
            Assert.Equal("", tuple.Item1.Service.MemberClass);
            Assert.Equal("", tuple.Item1.Service.MemberCode);
            Assert.Null(tuple.Item1.Service.SubsystemCode);
            Assert.Equal("", tuple.Item1.Service.ServiceCode);
            Assert.Null(tuple.Item1.Service.ServiceVersion);
        }

        [Fact]
        public void ReadAllElementsForServiceElement()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:subsystemCode /><id:serviceCode /><id:serviceVersion /></xrd:service>"));
            Assert.NotNull(tuple.Item1);
            Assert.NotNull(tuple.Item1.Service);
            Assert.Equal("", tuple.Item1.Service.XRoadInstance);
            Assert.Equal("", tuple.Item1.Service.MemberClass);
            Assert.Equal("", tuple.Item1.Service.MemberCode);
            Assert.Equal("", tuple.Item1.Service.SubsystemCode);
            Assert.Equal("", tuple.Item1.Service.ServiceCode);
            Assert.Equal("", tuple.Item1.Service.ServiceVersion);
        }

        [Fact]
        public void OptionalParameterAtWrongPositionForServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:serviceCode /><id:subsystemCode /></xrd:service>")));
            Assert.Equal("Unexpected element `{http://x-road.eu/xsd/identifiers}subsystemCode` in element `{http://x-road.eu/xsd/xroad.xsd}service`.", exception.Message);
        }

        [Fact]
        public void ReadSimpleOptionalElementValues()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<xrd:userId>Kalle</xrd:userId><xrd:issue>TOIMIK</xrd:issue>"));
            Assert.NotNull(tuple.Item1);
            Assert.Equal("Kalle", tuple.Item1.UserId);
            Assert.Equal("TOIMIK", tuple.Item1.Issue);
        }

        [Fact]
        public void EmptyCentralServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:centralService />")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` cannot be empty.", exception.Message);
        }

        [Fact]
        public void MissingObjectTypeAttributeForCentralServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:centralService></xrd:centralService>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.", exception.Message);
        }

        [Fact]
        public void MissingXRoadInstanceElementForCentralServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:centralService id:objectType=""CENTRALSERVICE""></xrd:centralService>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public void MissingServiceCodeElementForCentralServiceElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance /></xrd:centralService>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have child element `{http://x-road.eu/xsd/identifiers}serviceCode`.", exception.Message);
        }

        [Fact]
        public void ValidCentralServiceElement()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance>FI</id:xRoadInstance><id:serviceCode>fun</id:serviceCode></xrd:centralService>"));
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader40>(tuple.Item1);

            var xhr4 = (IXRoadHeader40)tuple.Item1;
            Assert.NotNull(xhr4.CentralService);
            Assert.Equal(XRoadObjectType.CentralService, xhr4.CentralService.ObjectType);
            Assert.Equal("FI", xhr4.CentralService.XRoadInstance);
            Assert.Equal("fun", xhr4.CentralService.ServiceCode);
        }

        [Fact]
        public void EmptyRepresentedPartyElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<repr:representedParty />")));
            Assert.Equal("Element `{http://x-road.eu/xsd/representation.xsd}representedParty` cannot be empty.", exception.Message);
        }

        [Fact]
        public void ElementPartyCodeIsRequiredForRepresentedPartyElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<repr:representedParty></repr:representedParty>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/representation.xsd}representedParty` must have child element `{http://x-road.eu/xsd/representation.xsd}partyCode`.", exception.Message);
        }

        [Fact]
        public void WrongElementOrderForRepresentedPartyElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(minimalValidHeader(@"<repr:representedParty><repr:partyCode /><repr:partyClass /></repr:representedParty>")));
            Assert.Equal("Unexpected element `{http://x-road.eu/xsd/representation.xsd}partyClass` in element `{http://x-road.eu/xsd/representation.xsd}representedParty`.", exception.Message);
        }

        [Fact]
        public void CanHandleMissingOptionalElementForRepresentedPartyElement()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<repr:representedParty><repr:partyCode /></repr:representedParty>"));
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader40>(tuple.Item1);

            var xhr4 = (IXRoadHeader40)tuple.Item1;
            Assert.NotNull(xhr4.RepresentedParty);
            Assert.Null(xhr4.RepresentedParty.Class);
            Assert.Equal("", xhr4.RepresentedParty.Code);
        }

        [Fact]
        public void CanHandleOptionalElementValueForRepresentedPartyElement()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>"));
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader40>(tuple.Item1);

            var xhr4 = (IXRoadHeader40)tuple.Item1;
            Assert.NotNull(xhr4.RepresentedParty);
            Assert.Equal("CLS", xhr4.RepresentedParty.Class);
            Assert.Equal("COD", xhr4.RepresentedParty.Code);
        }

        [Fact]
        public void RecognizesV4ProtocolFromRepresentedPartyElement()
        {
            var exception = Assert.Throws<XRoadException>(() => ParseHeader(@"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>"));
            Assert.Equal("X-Road header `client` element is mandatory.", exception.Message);
        }

        [Fact]
        public void WrongProtocolIsLeftUnresolved()
        {
            var tuple = ParseHeader(minimalValidHeader(@"<x:userId xmlns:x=""http://x-road.ee/xsd/x-road.xsd"">Mr. X</x:userId>"));
            Assert.NotNull(tuple.Item1);
            Assert.Same(Globals.XRoadProtocol40, tuple.Item3);
            Assert.Equal(1, tuple.Item2.Count);
            Assert.Equal("userId", tuple.Item2[0].Name.LocalName);
            Assert.Equal(NamespaceConstants.XROAD, tuple.Item2[0].Name.NamespaceName);
        }

        public static Tuple<IXRoadHeader, IList<XElement>, IXRoadProtocol> ParseHeader(string xml)
        {
            return ParseXRoadHeaderHelper.ParseHeader(xml, NamespaceConstants.XROAD_V4);
        }
    }
}
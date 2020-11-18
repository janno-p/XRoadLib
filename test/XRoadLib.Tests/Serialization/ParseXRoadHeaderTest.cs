using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XRoadLib.Headers;
using XRoadLib.Serialization;
using Xunit;

namespace XRoadLib.Tests.Serialization
{
    public class ParseXRoadHeaderTest
    {
        private static readonly Func<string, string> MinimalValidHeader = x => $"<xrd:client id:objectType=\"MEMBER\"><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion>{x}";

        [Fact]
        public async Task NoHeader()
        {
            var (soapHeader, elements, serviceManager) = await ParseHeaderAsync("");
            Assert.Null(soapHeader);
            Assert.Equal(0, elements.Count);
            Assert.Null(serviceManager);
        }

        [Fact]
        public async Task ValidatesPresenceOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync("<xrd:id>test</xrd:id>"));
            Assert.Equal("X-Road header `client` element is mandatory.", exception.Message);
        }

        [Fact]
        public async Task ValidatesContentOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync("<xrd:client />"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task ValidatesObjectTypeAttributeOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client></xrd:client>"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.", exception.Message);
        }

        [Fact]
        public async Task ValidatesXRoadInstanceSubelementOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""></xrd:client>"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public async Task ValidatesXRoadInstanceSubelementOfClientElementWithUnknownElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><x /></xrd:client>"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public async Task ValidatesMemberClassSubelementOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /></xrd:client>"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}memberClass`.", exception.Message);
        }

        [Fact]
        public async Task ValidatesMemberCodeSubelementOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /></xrd:client>"));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}client` must have child element `{http://x-road.eu/xsd/identifiers}memberCode`.", exception.Message);
        }

        [Fact]
        public async Task ValidatesInvalidSubelementOfClientElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /><x /></xrd:client>"));
            Assert.Equal("Unexpected element `{http://schemas.xmlsoap.org/soap/envelope/}x` in element `{http://x-road.eu/xsd/xroad.xsd}client`.", exception.Message);
        }

        [Fact]
        public async Task ValidatesPresenceOfIdElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client>"));
            Assert.Equal("X-Road header `id` element is mandatory.", exception.Message);
        }

        [Fact]
        public async Task ValidatesPresenceOfProtocolVersionElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id />"));
            Assert.Equal("X-Road header `protocolVersion` element is mandatory.", exception.Message);
        }

        [Fact]
        public async Task ValidatesValueOfProtocolVersionElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:client id:objectType=""MEMBER""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:client><xrd:id /><xrd:protocolVersion />"));
            Assert.Equal("Unsupported X-Road v6 protocol version value ``.", exception.Message);
        }

        [Fact]
        public async Task CollectsClientMandatoryValues()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(""));
            var header = tuple.Item1 as IXRoadHeader;
            Assert.NotNull(header);
            Assert.Same(Globals.ServiceManager, tuple.Item3);
            Assert.NotNull(header.Client);
            Assert.Equal("EE", header.Client.XRoadInstance);
            Assert.Equal("GOV", header.Client.MemberClass);
            Assert.Equal("12345", header.Client.MemberCode);
            Assert.Equal(XRoadObjectType.Member, header.Client.ObjectType);
            Assert.Null(header.Client.SubsystemCode);
            Assert.Equal("ABCDE", header.Id);
            Assert.Null(header.Issue);
            Assert.Equal("4.0", header.ProtocolVersion);
            Assert.Null(header.Service);
            Assert.Null(header.UserId);
        }

        [Fact]
        public async Task CollectsUnqualifiedSoapHeaderElementsButCannotDetectProtocolVersion()
        {
            var tuple = await ParseHeaderAsync(@"<x><test>bla</test></x><y /><z />");
            Assert.Null(tuple.Item1);
            Assert.Null(tuple.Item3);
            Assert.Equal(3, tuple.Item2.Count);

            var elX = tuple.Item2.SingleOrDefault(x => x.Name.LocalName == "x");
            Assert.NotNull(elX);

            var elXTest = elX.Element(XName.Get("test", NamespaceConstants.SoapEnv));
            Assert.NotNull(elXTest);
            Assert.Equal("bla", elXTest.Value);
            Assert.Contains(tuple.Item2, x => x.Name.LocalName == "y");
            Assert.Contains(tuple.Item2, x => x.Name.LocalName == "z");
        }

        [Fact]
        public async Task CollectsUnqualifiedSoapHeadersThatAreMixedWithXRoadElements()
        {
            var tuple = await ParseHeaderAsync(@"<x /><xrd:client id:objectType=""MEMBER""><id:xRoadInstance>EE</id:xRoadInstance><id:memberClass>GOV</id:memberClass><id:memberCode>12345</id:memberCode></xrd:client><y /><xrd:id>ABCDE</xrd:id><xrd:protocolVersion>4.0</xrd:protocolVersion><z />");
            var header = tuple.Item1 as IXRoadHeader;
            Assert.NotNull(header);
            Assert.Same(Globals.ServiceManager, tuple.Item3);
            Assert.NotNull(header.Client);
            Assert.Equal("EE", header.Client.XRoadInstance);
            Assert.Equal("GOV", header.Client.MemberClass);
            Assert.Equal("12345", header.Client.MemberCode);
            Assert.Equal(XRoadObjectType.Member, header.Client.ObjectType);
            Assert.Null(header.Client.SubsystemCode);
            Assert.Equal("ABCDE", header.Id);
            Assert.Null(header.Issue);
            Assert.Equal("4.0", header.ProtocolVersion);
            Assert.Null(header.Service);
            Assert.Null(header.UserId);
            Assert.Equal(3, tuple.Item2.Count);
            Assert.Contains(tuple.Item2, x => x.Name.LocalName == "x");
            Assert.Contains(tuple.Item2, x => x.Name.LocalName == "y");
            Assert.Contains(tuple.Item2, x => x.Name.LocalName == "z");
        }

        [Fact]
        public async Task UnrecognizedXRoadHeaderElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<xrd:x />"));
            Assert.Equal("Unexpected X-Road header element `{http://x-road.eu/xsd/xroad.xsd}x`.", exception.Message);
        }

        [Fact]
        public async Task EmptyServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service />")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task MissingObjectTypeAttributeForServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.", exception.Message);
        }

        [Fact]
        public async Task MissingXRoadInstanceElementForServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public async Task InvalidElementInServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><x /></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public async Task MissingMemberClassElementForServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}memberClass`.", exception.Message);
        }

        [Fact]
        public async Task MissingMemberCodeElementForServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}memberCode`.", exception.Message);
        }

        [Fact]
        public async Task MissingServiceCodeElementForServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /></xrd:service>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}service` must have child element `{http://x-road.eu/xsd/identifiers}serviceCode`.", exception.Message);
        }

        [Fact]
        public async Task ReadMinimalGroupOfElementsForServiceElement()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:serviceCode /></xrd:service>"));
            var header = tuple.Item1 as IXRoadHeader;
            Assert.NotNull(header);
            Assert.NotNull(header.Service);
            Assert.Equal("", header.Service.XRoadInstance);
            Assert.Equal("", header.Service.MemberClass);
            Assert.Equal("", header.Service.MemberCode);
            Assert.Null(header.Service.SubsystemCode);
            Assert.Equal("", header.Service.ServiceCode);
            Assert.Null(header.Service.ServiceVersion);
        }

        [Fact]
        public async Task ReadAllElementsForServiceElement()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:subsystemCode /><id:serviceCode /><id:serviceVersion /></xrd:service>"));
            var header = tuple.Item1 as IXRoadHeader;
            Assert.NotNull(header);
            Assert.NotNull(header.Service);
            Assert.Equal("", header.Service.XRoadInstance);
            Assert.Equal("", header.Service.MemberClass);
            Assert.Equal("", header.Service.MemberCode);
            Assert.Equal("", header.Service.SubsystemCode);
            Assert.Equal("", header.Service.ServiceCode);
            Assert.Equal("", header.Service.ServiceVersion);
        }

        [Fact]
        public async Task OptionalParameterAtWrongPositionForServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:service id:objectType=""SERVICE""><id:xRoadInstance /><id:memberClass /><id:memberCode /><id:serviceCode /><id:subsystemCode /></xrd:service>")));
            Assert.Equal("Unexpected element `{http://x-road.eu/xsd/identifiers}subsystemCode` in element `{http://x-road.eu/xsd/xroad.xsd}service`.", exception.Message);
        }

        [Fact]
        public async Task ReadSimpleOptionalElementValues()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(@"<xrd:userId>Kalle</xrd:userId><xrd:issue>TOIMIK</xrd:issue>"));
            var header = tuple.Item1 as IXRoadHeader;
            Assert.NotNull(header);
            Assert.Equal("Kalle", header.UserId);
            Assert.Equal("TOIMIK", header.Issue);
        }

        [Fact]
        public async Task EmptyCentralServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:centralService />")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task MissingObjectTypeAttributeForCentralServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:centralService></xrd:centralService>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have attribute `{http://x-road.eu/xsd/identifiers}objectType` value.", exception.Message);
        }

        [Fact]
        public async Task MissingXRoadInstanceElementForCentralServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:centralService id:objectType=""CENTRALSERVICE""></xrd:centralService>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have child element `{http://x-road.eu/xsd/identifiers}xRoadInstance`.", exception.Message);
        }

        [Fact]
        public async Task MissingServiceCodeElementForCentralServiceElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance /></xrd:centralService>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/xroad.xsd}centralService` must have child element `{http://x-road.eu/xsd/identifiers}serviceCode`.", exception.Message);
        }

        [Fact]
        public async Task ValidCentralServiceElement()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(@"<xrd:centralService id:objectType=""CENTRALSERVICE""><id:xRoadInstance>FI</id:xRoadInstance><id:serviceCode>fun</id:serviceCode></xrd:centralService>"));
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader>(tuple.Item1);

            var xhr4 = (IXRoadHeader)tuple.Item1;
            Assert.NotNull(xhr4.CentralService);
            Assert.Equal(XRoadObjectType.CentralService, xhr4.CentralService.ObjectType);
            Assert.Equal("FI", xhr4.CentralService.XRoadInstance);
            Assert.Equal("fun", xhr4.CentralService.ServiceCode);
        }

        [Fact]
        public async Task EmptyRepresentedPartyElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<repr:representedParty />")));
            Assert.Equal("Element `{http://x-road.eu/xsd/representation.xsd}representedParty` cannot be empty.", exception.Message);
        }

        [Fact]
        public async Task ElementPartyCodeIsRequiredForRepresentedPartyElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<repr:representedParty></repr:representedParty>")));
            Assert.Equal("Element `{http://x-road.eu/xsd/representation.xsd}representedParty` must have child element `{http://x-road.eu/xsd/representation.xsd}partyCode`.", exception.Message);
        }

        [Fact]
        public async Task WrongElementOrderForRepresentedPartyElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(MinimalValidHeader(@"<repr:representedParty><repr:partyCode /><repr:partyClass /></repr:representedParty>")));
            Assert.Equal("Unexpected element `{http://x-road.eu/xsd/representation.xsd}partyClass` in element `{http://x-road.eu/xsd/representation.xsd}representedParty`.", exception.Message);
        }

        [Fact]
        public async Task CanHandleMissingOptionalElementForRepresentedPartyElement()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(@"<repr:representedParty><repr:partyCode /></repr:representedParty>"));
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader>(tuple.Item1);

            var xhr4 = (IXRoadHeader)tuple.Item1;
            Assert.NotNull(xhr4.RepresentedParty);
            Assert.Null(xhr4.RepresentedParty.Class);
            Assert.Equal("", xhr4.RepresentedParty.Code);
        }

        [Fact]
        public async Task CanHandleOptionalElementValueForRepresentedPartyElement()
        {
            var tuple = await ParseHeaderAsync(MinimalValidHeader(@"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>"));
            Assert.NotNull(tuple.Item1);
            Assert.IsType<XRoadHeader>(tuple.Item1);

            var xhr4 = (IXRoadHeader)tuple.Item1;
            Assert.NotNull(xhr4.RepresentedParty);
            Assert.Equal("CLS", xhr4.RepresentedParty.Class);
            Assert.Equal("COD", xhr4.RepresentedParty.Code);
        }

        [Fact]
        public async Task RecognizesV4ProtocolFromRepresentedPartyElement()
        {
            var exception = await Assert.ThrowsAsync<InvalidQueryException>(() => ParseHeaderAsync(@"<repr:representedParty><repr:partyClass>CLS</repr:partyClass><repr:partyCode>COD</repr:partyCode></repr:representedParty>"));
            Assert.Equal("X-Road header `client` element is mandatory.", exception.Message);
        }

        [Fact]
        public async Task WrongProtocolIsLeftUnresolved()
        {
            var (header, unresolvedElements, serviceManager) = await ParseHeaderAsync(MinimalValidHeader(@"<x:userId xmlns:x=""http://x-road.ee/xsd/x-road.xsd"">Mr. X</x:userId>"));
            Assert.NotNull(header);
            Assert.Same(Globals.ServiceManager, serviceManager);
            Assert.Equal(1, unresolvedElements.Count);
            Assert.Equal("userId", unresolvedElements[0].Name.LocalName);
            Assert.Equal("http://x-road.ee/xsd/x-road.xsd", unresolvedElements[0].Name.NamespaceName);
        }

        private static Task<Tuple<ISoapHeader, IList<XElement>, IServiceManager>> ParseHeaderAsync(string xml)
        {
            return ParseXRoadHeaderHelper.ParseHeaderAsync(xml, NamespaceConstants.XRoad);
        }
    }
}
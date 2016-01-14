using System.Xml;
using XRoadLib.Extensions;

namespace XRoadLib.Header
{
    internal class XRoadHeader40 : XRoadHeaderBase, IXRoadHeader40
    {
        public override XRoadProtocol Protocol => XRoadProtocol.Version40;

        public XRoadCentralServiceIdentifier CentralService { get; private set; }
        public XRoadRepresentedParty RepresentedParty { get; private set; }

        public override void Validate()
        {
            if (Client == null)
                throw XRoadException.InvalidQuery("X-Road header `client` element is mandatory.");

            if (Id == null)
                throw XRoadException.InvalidQuery("X-Road header `id` element is mandatory.");

            if (ProtocolVersion == null)
                throw XRoadException.InvalidQuery("X-Road header `protocolVersion` element is mandatory.");
        }

        public override void SetHeaderValue(XmlReader reader)
        {
            if (reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR && reader.LocalName == "representedParty")
            {
                RepresentedParty = ReadRepresentedParty(reader);
                return;
            }

            if (reader.NamespaceURI == NamespaceConstants.XROAD_V4)
            {
                switch (reader.LocalName)
                {
                    case "client":
                        Client = ReadClient(reader);
                        return;

                    case "service":
                        Service = ReadService(reader);
                        return;

                    case "centralService":
                        CentralService = ReadCentralService(reader);
                        return;

                    case "id":
                        Id = reader.ReadInnerXml();
                        return;

                    case "userId":
                        UserId = reader.ReadInnerXml();
                        return;

                    case "issue":
                        Issue = reader.ReadInnerXml();
                        return;

                    case "protocolVersion":
                        ProtocolVersion = reader.ReadInnerXml();
                        return;
                }
            }

            throw XRoadException.InvalidQuery("Unexpected X-Road header element `{0}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI));
        }

        private static XRoadRepresentedParty ReadRepresentedParty(XmlReader reader)
        {
            var qualifiedName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery("Element `{0}` cannot be empty.", qualifiedName);

            var representedParty = new XRoadRepresentedParty();

            var depth = reader.Depth;
            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "partyClass" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR)
            {
                representedParty.Class = reader.ReadInnerXml();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("partyCode", NamespaceConstants.XROAD_V4_REPR));

            if (reader.LocalName == "partyCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR)
            {
                representedParty.Code = reader.ReadInnerXml();
                if (!reader.MoveToElement(depth + 1))
                    return representedParty;
            }

            throw XRoadException.InvalidQuery("Unexpected element `{0}` in element `{1}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), qualifiedName);
        }

        private static XRoadClientIdentifier ReadClient(XmlReader reader)
        {
            var qualifiedName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery("Element `{0}` cannot be empty.", qualifiedName);

            var client = new XRoadClientIdentifier();

            var depth = reader.Depth;

            if (!reader.MoveToAttribute("objectType", NamespaceConstants.XROAD_V4_ID))
                throw XRoadException.InvalidQuery("Element `{0}` must have attribute `{1}`.", qualifiedName, new XmlQualifiedName("objectType", NamespaceConstants.XROAD_V4_ID));
            client.ObjectType = GetObjectType(reader.ReadInnerXml());

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("xRoadInstance", NamespaceConstants.XROAD_V4_ID));
            client.XRoadInstance = reader.ReadInnerXml();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("memberClass", NamespaceConstants.XROAD_V4_ID));
            client.MemberClass = reader.ReadInnerXml();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("memberCode", NamespaceConstants.XROAD_V4_ID));
            client.MemberCode = reader.ReadInnerXml();

            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                client.SubsystemCode = reader.ReadInnerXml();
                success = reader.MoveToElement(depth + 1);
            }

            if (success)
                throw XRoadException.InvalidQuery("Unexpected element `{0}` in element `{1}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), qualifiedName);

            return client;
        }

        private static XRoadServiceIdentifier ReadService(XmlReader reader)
        {
            var qualifiedName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery("Element `{0}` cannot be empty.", qualifiedName);

            var service = new XRoadServiceIdentifier();

            var depth = reader.Depth;

            if (!reader.MoveToAttribute("objectType", NamespaceConstants.XROAD_V4_ID))
                throw XRoadException.InvalidQuery("Element `{0}` must have attribute `{1}`.", qualifiedName, new XmlQualifiedName("objectType", NamespaceConstants.XROAD_V4_ID));
            service.ObjectType = GetObjectType(reader.ReadInnerXml());

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("xRoadInstance", NamespaceConstants.XROAD_V4_ID));
            service.XRoadInstance = reader.ReadInnerXml();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("memberClass", NamespaceConstants.XROAD_V4_ID));
            service.MemberClass = reader.ReadInnerXml();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("memberCode", NamespaceConstants.XROAD_V4_ID));
            service.MemberCode = reader.ReadInnerXml();

            var success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                service.SubsystemCode = reader.ReadInnerXml();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("serviceCode", NamespaceConstants.XROAD_V4_ID));
            service.ServiceCode = reader.ReadInnerXml();

            success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "serviceVersion" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                service.ServiceVersion = reader.ReadInnerXml();
                success = reader.MoveToElement(depth + 1);
            }

            if (success)
                throw XRoadException.InvalidQuery("Unexpected element `{0}` in element `{1}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), qualifiedName);

            return service;
        }

        private static XRoadCentralServiceIdentifier ReadCentralService(XmlReader reader)
        {
            var qualifiedName = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery("Element `{0}` cannot be empty.", qualifiedName);

            var centralService = new XRoadCentralServiceIdentifier();

            var depth = reader.Depth;

            if (!reader.MoveToAttribute("objectType", NamespaceConstants.XROAD_V4_ID))
                throw XRoadException.InvalidQuery("Element `{0}` must have attribute `{1}`.", qualifiedName, new XmlQualifiedName("objectType", NamespaceConstants.XROAD_V4_ID));
            centralService.ObjectType = GetObjectType(reader.ReadInnerXml());

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("xRoadInstance", NamespaceConstants.XROAD_V4_ID));
            centralService.XRoadInstance = reader.ReadInnerXml();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery("Element `{0}` must have child element `{1}`.", qualifiedName, new XmlQualifiedName("serviceCode", NamespaceConstants.XROAD_V4_ID));
            centralService.ServiceCode = reader.ReadInnerXml();

            if (reader.MoveToElement(depth + 1))
                throw XRoadException.InvalidQuery("Unexpected element `{0}` in element `{1}`.", new XmlQualifiedName(reader.LocalName, reader.NamespaceURI), qualifiedName);

            return centralService;
        }

        private static XRoadObjectType GetObjectType(string value)
        {
            switch (value.Trim())
            {
                case "MEMBER":
                    return XRoadObjectType.Member;
                case "SUBSYSTEM":
                    return XRoadObjectType.Subsystem;
                case "SERVICE":
                    return XRoadObjectType.Service;
                case "CENTRALSERVICE":
                    return XRoadObjectType.CentralService;
                default:
                    throw XRoadException.InvalidQuery("Invalid `{0}` attribute value `{1}`.", new XmlQualifiedName("objectType", NamespaceConstants.XROAD_V4_ID), value);
            }
        }
    }
}
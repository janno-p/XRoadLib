using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;

namespace XRoadLib.Protocols.Headers
{
    public class XRoadHeader40 : IXRoadHeader, IXRoadHeader40
    {
        public XRoadClientIdentifier Client { get; set; }
        public XRoadServiceIdentifier Service { get; set; }
        public string UserId { get; set; }
        public string Issue { get; set; }
        public string Id { get; set; }
        public string ProtocolVersion { get; set; }

        public XRoadCentralServiceIdentifier CentralService { get; set; }
        public XRoadRepresentedParty RepresentedParty { get; set; }

        public void Validate()
        {
            if (Client == null)
                throw XRoadException.InvalidQuery("X-Road header `client` element is mandatory.");

            if (Id == null)
                throw XRoadException.InvalidQuery("X-Road header `id` element is mandatory.");

            if (ProtocolVersion == null)
                throw XRoadException.InvalidQuery("X-Road header `protocolVersion` element is mandatory.");
        }

        public void SetHeaderValue(XmlReader reader)
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
                        Id = reader.ReadElementContentAsString();
                        return;

                    case "userId":
                        UserId = reader.ReadElementContentAsString();
                        return;

                    case "issue":
                        Issue = reader.ReadElementContentAsString();
                        return;

                    case "protocolVersion":
                        ProtocolVersion = reader.ReadElementContentAsString();
                        return;
                }
            }

            throw XRoadException.InvalidQuery($"Unexpected X-Road header element `{reader.GetXName()}`.");
        }

        private static XRoadRepresentedParty ReadRepresentedParty(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` cannot be empty.");

            var representedParty = new XRoadRepresentedParty();

            var depth = reader.Depth;
            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "partyClass" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR)
            {
                representedParty.Class = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("partyCode", NamespaceConstants.XROAD_V4_REPR)}`.");

            if (reader.LocalName == "partyCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR)
            {
                representedParty.Code = reader.ReadElementContentAsString();
                if (!reader.MoveToElement(depth + 1))
                    return representedParty;
            }

            throw XRoadException.InvalidQuery($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");
        }

        private static XRoadClientIdentifier ReadClient(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` cannot be empty.");

            var client = new XRoadClientIdentifier();

            var depth = reader.Depth;

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XROAD_V4_ID);
            if (string.IsNullOrWhiteSpace(objectType))
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` value.");
            client.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID)}`.");
            client.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID)}`.");
            client.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID)}`.");
            client.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                client.SubsystemCode = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (success)
                throw XRoadException.InvalidQuery($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

            return client;
        }

        private static XRoadServiceIdentifier ReadService(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` cannot be empty.");

            var service = new XRoadServiceIdentifier();

            var depth = reader.Depth;

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XROAD_V4_ID);
            if (string.IsNullOrWhiteSpace(objectType))
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` value.");
            service.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID)}`.");
            service.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID)}`.");
            service.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID)}`.");
            service.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                service.SubsystemCode = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID)}`.");
            service.ServiceCode = reader.ReadElementContentAsString();

            success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "serviceVersion" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                service.ServiceVersion = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (success)
                throw XRoadException.InvalidQuery($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

            return service;
        }

        private static XRoadCentralServiceIdentifier ReadCentralService(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` cannot be empty.");

            var centralService = new XRoadCentralServiceIdentifier();

            var depth = reader.Depth;

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XROAD_V4_ID);
            if (string.IsNullOrWhiteSpace(objectType))
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` value.");
            centralService.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID)}`.");
            centralService.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw XRoadException.InvalidQuery($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID)}`.");
            centralService.ServiceCode = reader.ReadElementContentAsString();

            if (reader.MoveToElement(depth + 1))
                throw XRoadException.InvalidQuery($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

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
                    throw XRoadException.InvalidQuery($"Invalid `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` attribute value `{value}`.");
            }
        }
    }
}
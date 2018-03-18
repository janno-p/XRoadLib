using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Styles;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Implements default X-Road message protocol version 4.0 SOAP header.
    /// </summary>
    public class XRoadHeader40 : IXRoadHeader, IXRoadHeader<XRoadHeader40>, IXRoadHeader40
    {
        /// <summary>
        /// Client identity.
        /// </summary>
        public virtual XRoadClientIdentifier Client { get; set; }

        /// <summary>
        /// Service identity.
        /// </summary>
        public virtual XRoadServiceIdentifier Service { get; set; }

        /// <summary>
        /// User identity code.
        /// </summary>
        public virtual string UserId { get; set; }

        /// <summary>
        /// Received application, issue or document.
        /// </summary>
        public virtual string Issue { get; set; }

        /// <summary>
        /// Unique identity of the request.
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// X-Road message protocol version.
        /// </summary>
        public virtual string ProtocolVersion { get; set; }

        /// <summary>
        /// Central service identity.
        /// </summary>
        public virtual XRoadCentralServiceIdentifier CentralService { get; set; }

        /// <summary>
        /// Represented party details.
        /// </summary>
        public virtual XRoadRepresentedParty RepresentedParty { get; set; }

        /// <summary>
        /// Request hash of the X-Road message.
        /// </summary>
        public virtual XRoadRequestHash RequestHash { get; set; }

        /// <summary>
        /// Check for presence of mandatory parts.
        /// </summary>
        public virtual void Validate()
        {
            if (Client == null)
                throw new InvalidQueryException("X-Road header `client` element is mandatory.");

            if (Id == null)
                throw new InvalidQueryException("X-Road header `id` element is mandatory.");

            if (ProtocolVersion == null)
                throw new InvalidQueryException("X-Road header `protocolVersion` element is mandatory.");
        }

        /// <summary>
        /// Read next header value from the XML reader object.
        /// </summary>
        public virtual void ReadHeaderValue(XmlReader reader)
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

                    case "requestHash":
                        RequestHash = ReadRequestHash(reader);
                        return;
                }
            }

            throw new InvalidQueryException($"Unexpected X-Road header element `{reader.GetXName()}`.");
        }

        private static XRoadRepresentedParty ReadRepresentedParty(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

            var representedParty = new XRoadRepresentedParty();

            var depth = reader.Depth;
            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "partyClass" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR)
            {
                representedParty.Class = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("partyCode", NamespaceConstants.XROAD_V4_REPR)}`.");

            if (reader.LocalName == "partyCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_REPR)
            {
                representedParty.Code = reader.ReadElementContentAsString();
                if (!reader.MoveToElement(depth + 1))
                    return representedParty;
            }

            throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");
        }

        private static XRoadClientIdentifier ReadClient(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

            var client = new XRoadClientIdentifier();

            var depth = reader.Depth;

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XROAD_V4_ID);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` value.");
            client.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID)}`.");
            client.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID)}`.");
            client.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID)}`.");
            client.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                client.SubsystemCode = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

            return client;
        }

        private static XRoadServiceIdentifier ReadService(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

            var service = new XRoadServiceIdentifier();

            var depth = reader.Depth;

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XROAD_V4_ID);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` value.");
            service.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID)}`.");
            service.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID)}`.");
            service.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID)}`.");
            service.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                service.SubsystemCode = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID)}`.");
            service.ServiceCode = reader.ReadElementContentAsString();

            success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "serviceVersion" && reader.NamespaceURI == NamespaceConstants.XROAD_V4_ID)
            {
                service.ServiceVersion = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (success)
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

            return service;
        }

        private static XRoadCentralServiceIdentifier ReadCentralService(XmlReader reader)
        {
            var qualifiedName = reader.GetXName();

            if (reader.IsEmptyElement)
                throw new InvalidQueryException($"Element `{qualifiedName}` cannot be empty.");

            var centralService = new XRoadCentralServiceIdentifier();

            var depth = reader.Depth;

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XROAD_V4_ID);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` value.");
            centralService.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID)}`.");
            centralService.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XROAD_V4_ID)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID)}`.");
            centralService.ServiceCode = reader.ReadElementContentAsString();

            if (reader.MoveToElement(depth + 1))
                throw new InvalidQueryException($"Unexpected element `{reader.GetXName()}` in element `{qualifiedName}`.");

            return centralService;
        }

        private static XRoadRequestHash ReadRequestHash(XmlReader reader)
        {
            var algorithm = reader.GetAttribute("requestHash");
            var value = reader.ReadElementContentAsString();
            return new XRoadRequestHash(value, algorithm);
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
                    throw new InvalidQueryException($"Invalid `{XName.Get("objectType", NamespaceConstants.XROAD_V4_ID)}` attribute value `{value}`.");
            }
        }

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        public virtual void WriteTo(XmlWriter writer, Style style, HeaderDefinition definition)
        {
            if (writer.LookupPrefix(NamespaceConstants.XROAD_V4) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.XROAD, NamespaceConstants.XMLNS, NamespaceConstants.XROAD_V4);

            if (writer.LookupPrefix(NamespaceConstants.XROAD_V4_ID) == null)
                writer.WriteAttributeString("xmlns", PrefixConstants.ID, NamespaceConstants.XMLNS, NamespaceConstants.XROAD_V4_ID);

            if (definition.RequiredHeaders.Contains(XName.Get("client", NamespaceConstants.XROAD_V4)) || Client != null)
            {
                var element = new XElement(XName.Get("client", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), string.IsNullOrWhiteSpace(Client.SubsystemCode) ? "MEMBER" : "SUBSYSTEM"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), Client.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID), Client.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID), Client.MemberCode));
                if (!string.IsNullOrWhiteSpace(Client.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XROAD_V4_ID), Client.SubsystemCode));
                element.WriteTo(writer);
            }

            if (definition.RequiredHeaders.Contains(XName.Get("service", NamespaceConstants.XROAD_V4)) || Service != null)
            {
                var element = new XElement(XName.Get("service", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), "SERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), Service.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XROAD_V4_ID), Service.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XROAD_V4_ID), Service.MemberCode));
                if (!string.IsNullOrWhiteSpace(Service.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XROAD_V4_ID), Service.SubsystemCode));
                element.Add(new XElement(XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID), Service.ServiceCode));
                if (!string.IsNullOrWhiteSpace(Service.ServiceVersion))
                    element.Add(new XElement(XName.Get("serviceVersion", NamespaceConstants.XROAD_V4_ID), Service.ServiceVersion));
                element.WriteTo(writer);
            }

            if (definition.RequiredHeaders.Contains(XName.Get("centralService", NamespaceConstants.XROAD_V4)) || CentralService != null)
            {
                var element = new XElement(XName.Get("centralService", NamespaceConstants.XROAD_V4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XROAD_V4_ID), "CENTRALSERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XROAD_V4_ID), CentralService.XRoadInstance),
                    new XElement(XName.Get("serviceCode", NamespaceConstants.XROAD_V4_ID), CentralService.ServiceCode));
                element.WriteTo(writer);
            }

            void WriteHeaderValue(string elementName, object value, XName typeName)
            {
                var name = XName.Get(elementName, NamespaceConstants.XROAD_V4);
                if (definition.RequiredHeaders.Contains(name) || value != null) style.WriteHeaderElement(writer, name, value, typeName);
            }

            WriteHeaderValue("id", Id, XmlTypeConstants.String);
            WriteHeaderValue("userId", UserId, XmlTypeConstants.String);
            WriteHeaderValue("issue", Issue, XmlTypeConstants.String);
            WriteHeaderValue("protocolVersion", ProtocolVersion, XmlTypeConstants.String);
        }

        public XRoadHeader40 InitFrom(XRoadCommonHeader commonHeader)
        {
            Client = commonHeader.Client;
            Service = commonHeader.Service;
            UserId = commonHeader.UserId;
            Issue = commonHeader.Issue;
            ProtocolVersion = commonHeader.ProtocolVersion;
            Id = commonHeader.Id;

            return this;
        }
    }
}
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Implements default X-Road message protocol version 4.0 SOAP header.
    /// </summary>
    public class XRoadHeader : IXRoadHeader
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
            if (reader.NamespaceURI == NamespaceConstants.XRoadRepr && reader.LocalName == "representedParty")
            {
                RepresentedParty = ReadRepresentedParty(reader);
                return;
            }

            if (reader.NamespaceURI == NamespaceConstants.XRoad)
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

            if (success && reader.LocalName == "partyClass" && reader.NamespaceURI == NamespaceConstants.XRoadRepr)
            {
                representedParty.Class = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("partyCode", NamespaceConstants.XRoadRepr)}`.");

            if (reader.LocalName == "partyCode" && reader.NamespaceURI == NamespaceConstants.XRoadRepr)
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

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadId);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadId)}` value.");
            client.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadId)}`.");
            client.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XRoadId)}`.");
            client.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XRoadId)}`.");
            client.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XRoadId)
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

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadId);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadId)}` value.");
            service.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadId)}`.");
            service.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XRoadId)}`.");
            service.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XRoadId)}`.");
            service.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XRoadId)
            {
                service.SubsystemCode = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XRoadId)}`.");
            service.ServiceCode = reader.ReadElementContentAsString();

            success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "serviceVersion" && reader.NamespaceURI == NamespaceConstants.XRoadId)
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

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadId);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadId)}` value.");
            centralService.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadId)}`.");
            centralService.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XRoadId)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XRoadId)}`.");
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
                    throw new InvalidQueryException($"Invalid `{XName.Get("objectType", NamespaceConstants.XRoadId)}` attribute value `{value}`.");
            }
        }

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        public virtual void WriteTo(XmlWriter writer, Style style, HeaderDefinition definition)
        {
            if (writer.LookupPrefix(NamespaceConstants.XRoad) == null)
                writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.XRoad, NamespaceConstants.Xmlns, NamespaceConstants.XRoad);

            if (writer.LookupPrefix(NamespaceConstants.XRoadId) == null)
                writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.Id, NamespaceConstants.Xmlns, NamespaceConstants.XRoadId);

            if (definition.RequiredHeaders.Contains(XName.Get("client", NamespaceConstants.XRoad)) || Client != null)
            {
                var element = new XElement(XName.Get("client", NamespaceConstants.XRoad),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XRoadId), string.IsNullOrWhiteSpace(Client.SubsystemCode) ? "MEMBER" : "SUBSYSTEM"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XRoadId), Client.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XRoadId), Client.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XRoadId), Client.MemberCode));
                if (!string.IsNullOrWhiteSpace(Client.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XRoadId), Client.SubsystemCode));
                element.WriteTo(writer);
            }

            if (definition.RequiredHeaders.Contains(XName.Get("service", NamespaceConstants.XRoad)) || Service != null)
            {
                var element = new XElement(XName.Get("service", NamespaceConstants.XRoad),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XRoadId), "SERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XRoadId), Service.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XRoadId), Service.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XRoadId), Service.MemberCode));
                if (!string.IsNullOrWhiteSpace(Service.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XRoadId), Service.SubsystemCode));
                element.Add(new XElement(XName.Get("serviceCode", NamespaceConstants.XRoadId), Service.ServiceCode));
                if (!string.IsNullOrWhiteSpace(Service.ServiceVersion))
                    element.Add(new XElement(XName.Get("serviceVersion", NamespaceConstants.XRoadId), Service.ServiceVersion));
                element.WriteTo(writer);
            }

            if (definition.RequiredHeaders.Contains(XName.Get("centralService", NamespaceConstants.XRoad)) || CentralService != null)
            {
                var element = new XElement(XName.Get("centralService", NamespaceConstants.XRoad),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XRoadId), "CENTRALSERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XRoadId), CentralService.XRoadInstance),
                    new XElement(XName.Get("serviceCode", NamespaceConstants.XRoadId), CentralService.ServiceCode));
                element.WriteTo(writer);
            }

            void WriteHeaderValue(string elementName, object value, XName typeName)
            {
                var name = XName.Get(elementName, NamespaceConstants.XRoad);
                if (definition.RequiredHeaders.Contains(name) || value != null) style.WriteHeaderElement(writer, name, value, typeName);
            }

            WriteHeaderValue("id", Id, XmlTypeConstants.String);
            WriteHeaderValue("userId", UserId, XmlTypeConstants.String);
            WriteHeaderValue("issue", Issue, XmlTypeConstants.String);
            WriteHeaderValue("protocolVersion", ProtocolVersion, XmlTypeConstants.String);
        }
    }
}
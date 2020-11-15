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
    public class XRoadHeader40 : IXRoadHeader, IXRoadHeader40, IXRoadUniversalHeader
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
            if (reader.NamespaceURI == NamespaceConstants.XRoadV4Repr && reader.LocalName == "representedParty")
            {
                RepresentedParty = ReadRepresentedParty(reader);
                return;
            }

            if (reader.NamespaceURI == NamespaceConstants.XRoadV4)
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

            if (success && reader.LocalName == "partyClass" && reader.NamespaceURI == NamespaceConstants.XRoadV4Repr)
            {
                representedParty.Class = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("partyCode", NamespaceConstants.XRoadV4Repr)}`.");

            if (reader.LocalName == "partyCode" && reader.NamespaceURI == NamespaceConstants.XRoadV4Repr)
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

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadV4Id);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadV4Id)}` value.");
            client.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadV4Id)}`.");
            client.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XRoadV4Id)}`.");
            client.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XRoadV4Id)}`.");
            client.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);

            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XRoadV4Id)
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

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadV4Id);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadV4Id)}` value.");
            service.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadV4Id)}`.");
            service.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberClass" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberClass", NamespaceConstants.XRoadV4Id)}`.");
            service.MemberClass = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "memberCode" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("memberCode", NamespaceConstants.XRoadV4Id)}`.");
            service.MemberCode = reader.ReadElementContentAsString();

            var success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "subsystemCode" && reader.NamespaceURI == NamespaceConstants.XRoadV4Id)
            {
                service.SubsystemCode = reader.ReadElementContentAsString();
                success = reader.MoveToElement(depth + 1);
            }

            if (!success || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XRoadV4Id)}`.");
            service.ServiceCode = reader.ReadElementContentAsString();

            success = reader.MoveToElement(depth + 1);
            if (success && reader.LocalName == "serviceVersion" && reader.NamespaceURI == NamespaceConstants.XRoadV4Id)
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

            var objectType = reader.GetAttribute("objectType", NamespaceConstants.XRoadV4Id);
            if (string.IsNullOrWhiteSpace(objectType))
                throw new InvalidQueryException($"Element `{qualifiedName}` must have attribute `{XName.Get("objectType", NamespaceConstants.XRoadV4Id)}` value.");
            centralService.ObjectType = GetObjectType(objectType);

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "xRoadInstance" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("xRoadInstance", NamespaceConstants.XRoadV4Id)}`.");
            centralService.XRoadInstance = reader.ReadElementContentAsString();

            if (!reader.MoveToElement(depth + 1) || reader.LocalName != "serviceCode" || reader.NamespaceURI != NamespaceConstants.XRoadV4Id)
                throw new InvalidQueryException($"Element `{qualifiedName}` must have child element `{XName.Get("serviceCode", NamespaceConstants.XRoadV4Id)}`.");
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
                    throw new InvalidQueryException($"Invalid `{XName.Get("objectType", NamespaceConstants.XRoadV4Id)}` attribute value `{value}`.");
            }
        }

        /// <summary>
        /// Serializes X-Road message SOAP headers to XML.
        /// </summary>
        public virtual void WriteTo(XmlWriter writer, Style style, HeaderDefinition definition)
        {
            if (writer.LookupPrefix(NamespaceConstants.XRoadV4) == null)
                writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.XRoad, NamespaceConstants.Xmlns, NamespaceConstants.XRoadV4);

            if (writer.LookupPrefix(NamespaceConstants.XRoadV4Id) == null)
                writer.WriteAttributeString(PrefixConstants.Xmlns, PrefixConstants.Id, NamespaceConstants.Xmlns, NamespaceConstants.XRoadV4Id);

            if (definition.RequiredHeaders.Contains(XName.Get("client", NamespaceConstants.XRoadV4)) || Client != null)
            {
                var element = new XElement(XName.Get("client", NamespaceConstants.XRoadV4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XRoadV4Id), string.IsNullOrWhiteSpace(Client.SubsystemCode) ? "MEMBER" : "SUBSYSTEM"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XRoadV4Id), Client.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XRoadV4Id), Client.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XRoadV4Id), Client.MemberCode));
                if (!string.IsNullOrWhiteSpace(Client.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XRoadV4Id), Client.SubsystemCode));
                element.WriteTo(writer);
            }

            if (definition.RequiredHeaders.Contains(XName.Get("service", NamespaceConstants.XRoadV4)) || Service != null)
            {
                var element = new XElement(XName.Get("service", NamespaceConstants.XRoadV4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XRoadV4Id), "SERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XRoadV4Id), Service.XRoadInstance),
                    new XElement(XName.Get("memberClass", NamespaceConstants.XRoadV4Id), Service.MemberClass),
                    new XElement(XName.Get("memberCode", NamespaceConstants.XRoadV4Id), Service.MemberCode));
                if (!string.IsNullOrWhiteSpace(Service.SubsystemCode))
                    element.Add(new XElement(XName.Get("subsystemCode", NamespaceConstants.XRoadV4Id), Service.SubsystemCode));
                element.Add(new XElement(XName.Get("serviceCode", NamespaceConstants.XRoadV4Id), Service.ServiceCode));
                if (!string.IsNullOrWhiteSpace(Service.ServiceVersion))
                    element.Add(new XElement(XName.Get("serviceVersion", NamespaceConstants.XRoadV4Id), Service.ServiceVersion));
                element.WriteTo(writer);
            }

            if (definition.RequiredHeaders.Contains(XName.Get("centralService", NamespaceConstants.XRoadV4)) || CentralService != null)
            {
                var element = new XElement(XName.Get("centralService", NamespaceConstants.XRoadV4),
                    new XAttribute(XName.Get("objectType", NamespaceConstants.XRoadV4Id), "CENTRALSERVICE"),
                    new XElement(XName.Get("xRoadInstance", NamespaceConstants.XRoadV4Id), CentralService.XRoadInstance),
                    new XElement(XName.Get("serviceCode", NamespaceConstants.XRoadV4Id), CentralService.ServiceCode));
                element.WriteTo(writer);
            }

            void WriteHeaderValue(string elementName, object value, XName typeName)
            {
                var name = XName.Get(elementName, NamespaceConstants.XRoadV4);
                if (definition.RequiredHeaders.Contains(name) || value != null) style.WriteHeaderElement(writer, name, value, typeName);
            }

            WriteHeaderValue("id", Id, XmlTypeConstants.String);
            WriteHeaderValue("userId", UserId, XmlTypeConstants.String);
            WriteHeaderValue("issue", Issue, XmlTypeConstants.String);
            WriteHeaderValue("protocolVersion", ProtocolVersion, XmlTypeConstants.String);
        }

        string IXRoadUniversalHeader.Unit { get => null; set { } }
        string IXRoadUniversalHeader.Position { get => null; set { } }
        string IXRoadUniversalHeader.UserName { get => null; set { } }
        string IXRoadUniversalHeader.Authenticator { get => null; set { } }
        string IXRoadUniversalHeader.Paid { get => null; set { } }
        string IXRoadUniversalHeader.Encrypt { get => null; set { } }
        string IXRoadUniversalHeader.EncryptCert { get => null; set { } }
        string IXRoadUniversalHeader.Encrypted { get => null; set { } }
        string IXRoadUniversalHeader.EncryptedCert { get => null; set { } }

        bool? IXRoadUniversalHeader.Async { get => null; set { } }
    }
}
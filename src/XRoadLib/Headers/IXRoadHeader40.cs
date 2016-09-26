using System.Xml.Serialization;

namespace XRoadLib.Headers
{
    /// <summary>
    /// X-Road message protocol version 4.0 SOAP header elements.
    /// </summary>
    public interface IXRoadHeader40
    {
        /// <summary>
        /// Identifies a service client - an entity that initiates the service call.
        /// </summary>
        [XmlElement("client", Namespace = NamespaceConstants.XROAD_V4)]
        XRoadClientIdentifier Client { get; }

        /// <summary>
        /// Identifies the service that is invoked by the request.
        /// </summary>
        [XmlElement("service", Namespace = NamespaceConstants.XROAD_V4)]
        XRoadServiceIdentifier Service { get; } // Optional

        /// <summary>
        /// Identifies the central service that is invoked by the request.
        /// </summary>
        [XmlElement("centralService", Namespace = NamespaceConstants.XROAD_V4)]
        XRoadCentralServiceIdentifier CentralService { get; } // Optional

        /// <summary>
        /// Unique identifier for this message. The recommended form of message ID is UUID.
        /// </summary>
        [XmlElement("id", Namespace = NamespaceConstants.XROAD_V4)]
        string Id { get; }

        /// <summary>
        /// User whose action initiated the request. The user ID should be prefixed with twoletter
        /// ISO country code(e.g., EE12345678901).
        /// </summary>
        [XmlElement("userId", Namespace = NamespaceConstants.XROAD_V4)]
        string UserId { get; } // Optional

        /// <summary>
        /// Identifies received application, issue or document that was the cause of the
        /// service request.This field may be used by the client information system to connect
        /// service requests(and responses) to working procedures.
        /// </summary>
        [XmlElement("issue", Namespace = NamespaceConstants.XROAD_V4)]
        string Issue { get; } // Optional

        /// <summary>
        /// X-Road message protocol version. The value of this field MUST be 4.0
        /// </summary>
        [XmlElement("protocolVersion", Namespace = NamespaceConstants.XROAD_V4)]
        string ProtocolVersion { get; }

        /// <summary>
        /// Base64 encoded hash of the SOAP request message
        /// </summary>
        [XmlElement("requestHash", Namespace = NamespaceConstants.XROAD_V4)]
        XRoadRequestHash RequestHash { get; } // For responses only

        /// <summary>
        /// Identifies a party that is being represented in a service request.
        /// </summary>
        [XmlElement("representedParty", Namespace = NamespaceConstants.XROAD_V4_REPR)]
        XRoadRepresentedParty RepresentedParty { get; } // Optional
    }
}
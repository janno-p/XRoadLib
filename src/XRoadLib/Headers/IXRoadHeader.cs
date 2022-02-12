namespace XRoadLib.Headers;

/// <summary>
/// Describes X-Road message SOAP header elements.
/// </summary>
public interface IXRoadHeader : ISoapHeader
{
    /// <summary>
    /// Identifies a service client - an entity that initiates the service call.
    /// </summary>
    [XmlElement("client", Namespace = NamespaceConstants.XRoad)]
    XRoadClientIdentifier Client { get; }

    /// <summary>
    /// Identifies the service that is invoked by the request.
    /// </summary>
    [XmlElement("service", Namespace = NamespaceConstants.XRoad)]
    XRoadServiceIdentifier Service { get; } // Optional

    /// <summary>
    /// Identifies the central service that is invoked by the request.
    /// </summary>
    [XmlElement("centralService", Namespace = NamespaceConstants.XRoad)]
    XRoadCentralServiceIdentifier CentralService { get; } // Optional

    /// <summary>
    /// User whose action initiated the request. The user ID should be prefixed with twoletter
    /// ISO country code(e.g., EE12345678901).
    /// </summary>
    [XmlElement("userId", Namespace = NamespaceConstants.XRoad)]
    string UserId { get; } // Optional

    /// <summary>
    /// Unique identifier for this message. The recommended form of message ID is UUID.
    /// </summary>
    [XmlElement("id", Namespace = NamespaceConstants.XRoad)]
    string Id { get; }

    /// <summary>
    /// Identifies received application, issue or document that was the cause of the
    /// service request.This field may be used by the client information system to connect
    /// service requests(and responses) to working procedures.
    /// </summary>
    [XmlElement("issue", Namespace = NamespaceConstants.XRoad)]
    string Issue { get; } // Optional

    /// <summary>
    /// X-Road message protocol version. The value of this field MUST be 4.0
    /// </summary>
    [XmlElement("protocolVersion", Namespace = NamespaceConstants.XRoad)]
    string ProtocolVersion { get; }

    /// <summary>
    /// Base64 encoded hash of the SOAP request message
    /// </summary>
    [XmlElement("requestHash", Namespace = NamespaceConstants.XRoad)]
    XRoadRequestHash RequestHash { get; } // For responses only

    /// <summary>
    /// Identifies a party that is being represented in a service request.
    /// </summary>
    [XmlElement("representedParty", Namespace = NamespaceConstants.XRoadRepr)]
    XRoadRepresentedParty RepresentedParty { get; } // Optional

    /// <summary>
    /// Try to read current position in XML reader as X-Road header element.
    /// </summary>
    Task ReadHeaderValueAsync(XmlReader reader);

    /// <summary>
    /// Check if all required SOAP headers are present and in correct format.
    /// </summary>
    void Validate();
}
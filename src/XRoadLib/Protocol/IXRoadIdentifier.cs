using System.Xml.Serialization;

namespace XRoadLib.Protocol
{
    /// <summary>
    /// Globally unique identifier in the XRoad system. Identifier consists of object type specifier and list of
    /// hierarchical codes (starting with code that identifiers the XRoad instance).
    /// </summary>
    [XmlType("XRoadIdentifierType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public interface IXRoadIdentifier
    {
        /// <summary>
        /// Identifies the XRoad instance. This field is applicable to all identifier types.
        /// </summary>
        [XmlElement("xRoadInstance", Namespace = NamespaceConstants.XROAD_V4_ID)]
        string XRoadInstance { get; }

        /// <summary>
        /// Type of the member (company, government institution, private person, etc.)
        /// </summary>
        [XmlElement("memberClass", Namespace = NamespaceConstants.XROAD_V4_ID)]
        string MemberClass { get; }

        /// <summary>
        /// Code that uniquely identifies a member of given member type.
        /// </summary>
        [XmlElement("memberCode", Namespace = NamespaceConstants.XROAD_V4_ID)]
        string MemberCode { get; }

        /// <summary>
        /// Code that uniquely identifies a subsystem of given XRoad member.
        /// </summary>
        [XmlElement("subsystemCode", Namespace = NamespaceConstants.XROAD_V4_ID)]
        string SubsystemCode { get; }

        /// <summary>
        /// Code that uniquely identifies a service offered by given XRoad member or subsystem.
        /// </summary>
        [XmlElement("serviceCode", Namespace = NamespaceConstants.XROAD_V4_ID)]
        string ServiceCode { get; }

        /// <summary>
        /// Version of the service.
        /// </summary>
        [XmlElement("serviceVersion", Namespace = NamespaceConstants.XROAD_V4_ID)]
        string ServiceVersion { get; }

        [XmlAttribute("objectType", Namespace = NamespaceConstants.XROAD_V4_ID)]
        XRoadObjectType ObjectType { get; }
    }
}
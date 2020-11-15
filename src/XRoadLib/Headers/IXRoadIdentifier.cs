using System.Xml.Serialization;

namespace XRoadLib.Headers
{
    /// <summary>
    /// Globally unique identifier in the XRoad system. Identifier consists of object type specifier and list of
    /// hierarchical codes (starting with code that identifiers the XRoad instance).
    /// </summary>
    [XmlType("XRoadIdentifierType", Namespace = NamespaceConstants.XRoadId)]
    public interface IXRoadIdentifier
    {
        /// <summary>
        /// Identifies the XRoad instance. This field is applicable to all identifier types.
        /// </summary>
        [XmlElement("xRoadInstance", Namespace = NamespaceConstants.XRoadId)]
        string XRoadInstance { get; }

        /// <summary>
        /// Type of the member (company, government institution, private person, etc.)
        /// </summary>
        [XmlElement("memberClass", Namespace = NamespaceConstants.XRoadId)]
        string MemberClass { get; }

        /// <summary>
        /// Code that uniquely identifies a member of given member type.
        /// </summary>
        [XmlElement("memberCode", Namespace = NamespaceConstants.XRoadId)]
        string MemberCode { get; }

        /// <summary>
        /// Code that uniquely identifies a subsystem of given XRoad member.
        /// </summary>
        [XmlElement("subsystemCode", Namespace = NamespaceConstants.XRoadId)]
        string SubsystemCode { get; }

        /// <summary>
        /// Code that uniquely identifies a service offered by given XRoad member or subsystem.
        /// </summary>
        [XmlElement("serviceCode", Namespace = NamespaceConstants.XRoadId)]
        string ServiceCode { get; }

        /// <summary>
        /// Version of the service.
        /// </summary>
        [XmlElement("serviceVersion", Namespace = NamespaceConstants.XRoadId)]
        string ServiceVersion { get; }

        /// <summary>
        /// Specifies identifier type.
        /// </summary>
        [XmlAttribute("objectType", Namespace = NamespaceConstants.XRoadId)]
        XRoadObjectType ObjectType { get; }
    }
}
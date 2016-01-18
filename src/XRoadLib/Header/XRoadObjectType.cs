using System.Xml.Serialization;

namespace XRoadLib.Header
{
    /// <summary>
    /// Enumeration for XRoad identifier types.
    /// </summary>
    [XmlType("XRoadObjectType", Namespace = NamespaceConstants.XROAD_V4_ID)]
    public enum XRoadObjectType
    {
        [XmlEnum("MEMBER")]
        Member,

        [XmlEnum("SUBSYSTEM")]
        Subsystem,

        [XmlEnum("SERVICE")]
        Service,

        [XmlEnum("CENTRALSERVICE")]
        CentralService
    }
}
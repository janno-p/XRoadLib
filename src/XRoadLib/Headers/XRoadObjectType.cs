namespace XRoadLib.Headers;

/// <summary>
/// Enumeration for XRoad identifier types.
/// </summary>
[XmlType("XRoadObjectType", Namespace = NamespaceConstants.XRoadId)]
public enum XRoadObjectType
{
    /// <summary>
    /// Identifies member identifier.
    /// </summary>
    [XmlEnum("MEMBER")]
    Member,

    /// <summary>
    /// Identifies subsystem identifier.
    /// </summary>
    [XmlEnum("SUBSYSTEM")]
    Subsystem,

    /// <summary>
    /// Identifies service identifier.
    /// </summary>
    [XmlEnum("SERVICE")]
    Service,

    /// <summary>
    /// Identifies central service identifier.
    /// </summary>
    [XmlEnum("CENTRALSERVICE")]
    CentralService
}
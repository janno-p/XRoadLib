namespace XRoadLib;

/// <summary>
/// Predefined XML types.
/// </summary>
public static class XmlTypeConstants
{
    /// <summary>
    /// Standard XML Schema string type name.
    /// </summary>
    public static readonly XName String = XName.Get("string", NamespaceConstants.Xsd);

    /// <summary>
    /// Standard XML Schema boolean type name.
    /// </summary>
    public static readonly XName Boolean = XName.Get("boolean", NamespaceConstants.Xsd);

    /// <summary>
    /// Standard XML Schema base64 type name.
    /// </summary>
    public static readonly XName Base64 = XName.Get("base64", NamespaceConstants.Xsd);
}
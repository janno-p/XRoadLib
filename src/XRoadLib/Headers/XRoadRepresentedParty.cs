namespace XRoadLib.Headers;

/// <summary>
/// X-Road header extension for represented party messages.
/// </summary>
[XmlType("XRoadRepresentedPartyType", Namespace = NamespaceConstants.XRoadRepr)]
public class XRoadRepresentedParty
{
    /// <summary>
    /// Class of the represented party.
    /// </summary>
    [XmlElement("partyClass", Namespace = NamespaceConstants.XRoadRepr, Order = 1)]
    public string Class { get; internal set; } // Optional

    /// <summary>
    /// Code of the represented party.
    /// </summary>
    [XmlElement("partyCode", Namespace = NamespaceConstants.XRoadRepr, Order = 2)]
    public string Code { get; internal set; }
}
using System.Xml.Serialization;

namespace XRoadLib.Protocols.Headers
{
    [XmlType("XRoadRepresentedPartyType", Namespace = NamespaceConstants.XROAD_V4_REPR)]
    public class XRoadRepresentedParty
    {
        /// <summary>
        /// Class of the represented party.
        /// </summary>
        [XmlElement("partyClass", Namespace = NamespaceConstants.XROAD_V4_REPR, Order = 1)]
        public string Class { get; internal set; } // Optional

        /// <summary>
        /// Code of the represented party.
        /// </summary>
        [XmlElement("partyCode", Namespace = NamespaceConstants.XROAD_V4_REPR, Order = 2)]
        public string Code { get; internal set; }
    }
}
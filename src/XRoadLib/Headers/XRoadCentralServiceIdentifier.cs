using System;
using System.Xml.Serialization;

namespace XRoadLib.Headers
{
    /// <summary>
    /// X-Road central service identifier.
    /// </summary>
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadCentralServiceIdentifierType", Namespace = NamespaceConstants.XRoadId)]
    public class XRoadCentralServiceIdentifier : IXRoadIdentifier
    {
        /// <summary>
        /// Specifies X-Road instance.
        /// </summary>
        public string XRoadInstance { get; set; }

        string IXRoadIdentifier.MemberClass => throw new NotImplementedException();
        string IXRoadIdentifier.MemberCode => throw new NotImplementedException();
        string IXRoadIdentifier.SubsystemCode => throw new NotImplementedException();

        /// <summary>
        /// X-Road central service name.
        /// </summary>
        public string ServiceCode { get; set; }

        string IXRoadIdentifier.ServiceVersion => throw new NotImplementedException();

        /// <summary>
        /// Must be assigned to CENTRALSERVICE.
        /// </summary>
        public XRoadObjectType ObjectType { get; set; }

        /// <summary>
        /// String presentation of the central service identifier.
        /// </summary>
        public override string ToString()
        {
            return $"{XRoadInstance}/{ServiceCode}";
        }
    }
}
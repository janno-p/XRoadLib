using System;
using System.Text;
using System.Xml.Serialization;

namespace XRoadLib.Headers
{
    /// <summary>
    /// X-Road service client identifier.
    /// </summary>
    [XmlInclude(typeof(IXRoadIdentifier))]
    [XmlType("XRoadClientIdentifierType", Namespace = NamespaceConstants.XRoadId)]
    public class XRoadClientIdentifier : IXRoadIdentifier
    {
        /// <summary>
        /// X-Road instance name.
        /// </summary>
        public string XRoadInstance { get; set; }

        /// <summary>
        /// Clients member class.
        /// </summary>
        public string MemberClass { get; set; }

        /// <summary>
        /// Clients unique code.
        /// </summary>
        public string MemberCode { get; set; }

        /// <summary>
        /// Subsystem name, when client identifier specifies subsystem identifier.
        /// </summary>
        public string SubsystemCode { get; set; } // Optional

        string IXRoadIdentifier.ServiceCode => throw new NotImplementedException();
        string IXRoadIdentifier.ServiceVersion => throw new NotImplementedException();

        /// <summary>
        /// X-Road client identifier type: MEMBER or SUBSYSTEM.
        /// </summary>
        public XRoadObjectType ObjectType { get; set; }

        /// <summary>
        /// String presentation of the client identifier.
        /// </summary>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(XRoadInstance) && string.IsNullOrEmpty(MemberClass) && string.IsNullOrEmpty(SubsystemCode))
                return MemberCode ?? "";

            var sb = new StringBuilder($"{XRoadInstance}/{MemberClass}/{MemberCode}");
            if (!string.IsNullOrEmpty(SubsystemCode))
                sb.Append($"/{SubsystemCode}");

            return sb.ToString();
        }
    }
}
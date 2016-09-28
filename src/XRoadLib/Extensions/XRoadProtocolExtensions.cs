using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Headers;
using XRoadLib.Serialization;

namespace XRoadLib.Extensions
{
    /// <summary>
    /// Extension methods of XRoadProtocol type.
    /// </summary>
    public static class XRoadProtocolExtensions
    {
        /// <summary>
        /// Initialize new X-Road message of this X-Road message protocol instance.
        /// </summary>
        public static XRoadMessage CreateMessage(this IXRoadProtocol protocol, IXRoadHeader header = null) =>
            new XRoadMessage(protocol, header ?? protocol.HeaderDefinition.CreateInstance());

        /// <summary>
        /// Serializes header of SOAP message.
        /// </summary>
        public static void WriteSoapHeader(this IXRoadProtocol protocol, XmlWriter writer, IXRoadHeader header, IEnumerable<XElement> additionalHeaders = null)
        {
            writer.WriteStartElement("Header", NamespaceConstants.SOAP_ENV);

            header?.WriteTo(writer, protocol);

            foreach (var additionalHeader in additionalHeaders ?? Enumerable.Empty<XElement>())
                additionalHeader.WriteTo(writer);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Test if given namespace is defined as SOAP header element namespace.
        /// </summary>
        public static bool IsHeaderNamespace(this IXRoadProtocol protocol, string namespaceName) =>
            protocol.HeaderDefinition.IsHeaderNamespace(namespaceName);

        /// <summary>
        /// Check if envelope defines given protocol schema.
        /// </summary>
        public static bool IsDefinedByEnvelope(this IXRoadProtocol protocol, XmlReader reader) =>
            protocol.ProtocolDefinition.DetectEnvelope?.Invoke(reader) ?? false;
    }
}
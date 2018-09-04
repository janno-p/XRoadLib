using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Styles;

namespace XRoadLib.Extensions
{
    /// <summary>
    /// Extension methods for <see>XmlWriter</see> type.
    /// </summary>
    public static class XmlWriterExtensions
    {
        private static readonly XmlQualifiedName qnXsiType = new XmlQualifiedName("type", NamespaceConstants.XSI);

        /// <summary>
        /// Serializes attribute with qualified name content.
        /// </summary>
        public static void WriteQualifiedAttribute(this XmlWriter writer, string name, XmlQualifiedName qualifiedName)
        {
            if (qualifiedName == null || qualifiedName.IsEmpty)
                return;

            writer.WriteStartAttribute(name);
            writer.WriteQualifiedName(qualifiedName.Name, qualifiedName.Namespace);
            writer.WriteEndAttribute();
        }

        private static void WriteQualifiedAttribute(this XmlWriter writer, string attributeName, string attributeNamespace, string valueName, string valueNamespace)
        {
            writer.WriteStartAttribute(attributeName, attributeNamespace);
            writer.WriteQualifiedName(valueName, valueNamespace);
            writer.WriteEndAttribute();
        }

        /// <summary>
        /// Serializes xsi:type attribute.
        /// </summary>
        public static void WriteTypeAttribute(this XmlWriter writer, string typeName, string typeNamespace)
        {
            writer.WriteQualifiedAttribute(qnXsiType.Name, qnXsiType.Namespace, typeName, typeNamespace);
        }

        /// <summary>
        /// Serializes xsi:type attribute.
        /// </summary>
        public static void WriteTypeAttribute(this XmlWriter writer, XName qualifiedName, string ns = null)
        {
            writer.WriteTypeAttribute(qualifiedName.LocalName, ns ?? qualifiedName.NamespaceName);
        }

        private static void WriteArrayTypeAttribute(this XmlWriter writer, string typeName, string typeNamespace, int arraySize)
        {
            writer.WriteStartAttribute("arrayType", NamespaceConstants.SOAP_ENC);
            writer.WriteQualifiedName(typeName, typeNamespace);
            writer.WriteString($"[{arraySize}]");
            writer.WriteEndAttribute();
        }

        /// <summary>
        /// Serializes SOAP encoded array type attribute.
        /// </summary>
        public static void WriteArrayTypeAttribute(this XmlWriter writer, XName qualifiedName, int arraySize)
        {
            writer.WriteArrayTypeAttribute(qualifiedName.LocalName, qualifiedName.NamespaceName, arraySize);
        }

        /// <summary>
        /// Serializes xsi:nil element attribute.
        /// </summary>
        public static void WriteNilAttribute(this XmlWriter writer)
        {
            writer.WriteAttributeString("nil", NamespaceConstants.XSI, "1");
        }

        private static void WriteCDataEscape(this XmlWriter writer, string value)
        {
            if (!value.Contains("&") && !value.Contains("<") && !value.Contains(">"))
            {
                writer.WriteValue(value);
                return;
            }

            var startIndex = 0;
            while (true)
            {
                var endIndex = value.IndexOf("]]>", startIndex, StringComparison.Ordinal);
                if (endIndex < 0)
                {
                    writer.WriteCData(value.Substring(startIndex));
                    break;
                }

                writer.WriteCData(value.Substring(startIndex, endIndex - startIndex));
                writer.WriteRaw("]]");

                startIndex = endIndex + 2;
            }
        }

        /// <summary>
        /// Serializes string value with required serialization mode.
        /// </summary>
        public static void WriteStringWithMode(this XmlWriter writer, string value, StringSerializationMode mode)
        {
            if (mode == StringSerializationMode.HtmlEncoded)
                writer.WriteString(value);
            else writer.WriteCDataEscape(value);
        }

        public static void WriteStartElement(this XmlWriter writer, XName name)
        {
            writer.WriteStartElement(name.LocalName, name.NamespaceName);
        }

        /// <summary>
        /// Serializes beginning of SOAP envelope into X-Road message.
        /// </summary>
        public static void WriteSoapEnvelope(this XmlWriter writer, ProtocolDefinition protocolDefinition)
        {
            var soapEnvPrefix = protocolDefinition.GlobalNamespacePrefixes[NamespaceConstants.SOAP_ENV];

            writer.WriteStartElement(soapEnvPrefix, "Envelope", NamespaceConstants.SOAP_ENV);

            foreach (var kvp in protocolDefinition.GlobalNamespacePrefixes)
                writer.WriteAttributeString(PrefixConstants.XMLNS, kvp.Value, NamespaceConstants.XMLNS, kvp.Key.NamespaceName);

            if (protocolDefinition.Style is RpcEncodedStyle)
                writer.WriteAttributeString("encodingStyle", NamespaceConstants.SOAP_ENV, NamespaceConstants.SOAP_ENC);
        }

        public static void WriteMissingAttributes(this XmlWriter writer, ProtocolDefinition protocolDefinition)
        {
            foreach (var kvp in protocolDefinition.GlobalNamespacePrefixes)
                if (writer.LookupPrefix(kvp.Key.NamespaceName) == null)
                    writer.WriteAttributeString(PrefixConstants.XMLNS, kvp.Value, NamespaceConstants.XMLNS, kvp.Key.NamespaceName);
        }
    }
}
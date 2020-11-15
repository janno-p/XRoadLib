using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Schema;
using XRoadLib.Serialization;
using XRoadLib.Soap;
using XRoadLib.Styles;

namespace XRoadLib.Extensions
{
    /// <summary>
    /// Extension methods for <see>XmlWriter</see> type.
    /// </summary>
    public static class XmlWriterExtensions
    {
        private static readonly XmlQualifiedName QnXsiType = new XmlQualifiedName("type", NamespaceConstants.Xsi);

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
            writer.WriteQualifiedAttribute(QnXsiType.Name, QnXsiType.Namespace, typeName, typeNamespace);
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
            writer.WriteStartAttribute("arrayType", NamespaceConstants.SoapEnc);
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
            writer.WriteAttributeString("nil", NamespaceConstants.Xsi, "1");
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
        public static void WriteSoapEnvelope(this XmlWriter writer, IMessageFormatter messageFormatter, ProtocolDefinition protocolDefinition)
        {
            var soapEnvPrefix = protocolDefinition != null ? protocolDefinition.GlobalNamespacePrefixes[messageFormatter.Namespace] : PrefixConstants.SoapEnv;

            messageFormatter.WriteStartEnvelope(writer, soapEnvPrefix);

            if (protocolDefinition == null)
                return;

            foreach (var kvp in protocolDefinition.GlobalNamespacePrefixes)
                writer.WriteAttributeString(PrefixConstants.Xmlns, kvp.Value, NamespaceConstants.Xmlns, kvp.Key.NamespaceName);

            if (protocolDefinition.Style is RpcEncodedStyle)
                writer.WriteAttributeString("encodingStyle", messageFormatter.Namespace, NamespaceConstants.SoapEnc);
        }

        public static void WriteMissingAttributes(this XmlWriter writer, ProtocolDefinition protocolDefinition)
        {
            if (protocolDefinition == null)
                return;

            foreach (var kvp in protocolDefinition.GlobalNamespacePrefixes)
                if (writer.LookupPrefix(kvp.Key.NamespaceName) == null)
                    writer.WriteAttributeString(PrefixConstants.Xmlns, kvp.Value, NamespaceConstants.Xmlns, kvp.Key.NamespaceName);
        }
    }
}
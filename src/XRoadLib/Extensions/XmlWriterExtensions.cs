﻿using System;
using System.Threading.Tasks;
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
        private static readonly XmlQualifiedName QnXsiType = new("type", NamespaceConstants.Xsi);

        /// <summary>
        /// Serializes attribute with qualified name content.
        /// </summary>
        public static async Task WriteQualifiedAttributeAsync(this XmlWriter writer, string name, XmlQualifiedName qualifiedName)
        {
            if (qualifiedName == null || qualifiedName.IsEmpty)
                return;

            writer.WriteStartAttribute(name);
            await writer.WriteQualifiedNameAsync(qualifiedName.Name, qualifiedName.Namespace).ConfigureAwait(false);
            writer.WriteEndAttribute();
        }

        private static async Task WriteQualifiedAttributeAsync(this XmlWriter writer, string attributeName, string attributeNamespace, string valueName, string valueNamespace)
        {
            writer.WriteStartAttribute(attributeName, attributeNamespace);
            await writer.WriteQualifiedNameAsync(valueName, valueNamespace).ConfigureAwait(false);
            writer.WriteEndAttribute();
        }

        /// <summary>
        /// Serializes xsi:type attribute.
        /// </summary>
        public static Task WriteTypeAttributeAsync(this XmlWriter writer, string typeName, string typeNamespace)
        {
            return writer.WriteQualifiedAttributeAsync(QnXsiType.Name, QnXsiType.Namespace, typeName, typeNamespace);
        }

        /// <summary>
        /// Serializes xsi:type attribute.
        /// </summary>
        public static Task WriteTypeAttributeAsync(this XmlWriter writer, XName qualifiedName, string ns = null)
        {
            return writer.WriteTypeAttributeAsync(qualifiedName.LocalName, ns ?? qualifiedName.NamespaceName);
        }

        private static async Task WriteArrayTypeAttributeAsync(this XmlWriter writer, string typeName, string typeNamespace, int arraySize)
        {
            writer.WriteStartAttribute("arrayType", NamespaceConstants.SoapEnc);
            await writer.WriteQualifiedNameAsync(typeName, typeNamespace).ConfigureAwait(false);
            await writer.WriteStringAsync($"[{arraySize}]").ConfigureAwait(false);
            writer.WriteEndAttribute();
        }

        /// <summary>
        /// Serializes SOAP encoded array type attribute.
        /// </summary>
        public static Task WriteArrayTypeAttributeAsync(this XmlWriter writer, XName qualifiedName, int arraySize)
        {
            return writer.WriteArrayTypeAttributeAsync(qualifiedName.LocalName, qualifiedName.NamespaceName, arraySize);
        }

        /// <summary>
        /// Serializes xsi:nil element attribute.
        /// </summary>
        public static Task WriteNilAttributeAsync(this XmlWriter writer)
        {
            return writer.WriteAttributeStringAsync(null, "nil", NamespaceConstants.Xsi, "1");
        }

        private static async Task WriteCDataEscapeAsync(this XmlWriter writer, string value)
        {
            if (!value.Contains("&") && !value.Contains("<") && !value.Contains(">"))
            {
                await writer.WriteStringAsync(value).ConfigureAwait(false);
                return;
            }

            var startIndex = 0;
            while (true)
            {
                var endIndex = value.IndexOf("]]>", startIndex, StringComparison.Ordinal);
                if (endIndex < 0)
                {
                    await writer.WriteCDataAsync(value.Substring(startIndex)).ConfigureAwait(false);
                    break;
                }

                await writer.WriteCDataAsync(value.Substring(startIndex, endIndex - startIndex)).ConfigureAwait(false);
                await writer.WriteRawAsync("]]").ConfigureAwait(false);

                startIndex = endIndex + 2;
            }
        }

        /// <summary>
        /// Serializes string value with required serialization mode.
        /// </summary>
        public static Task WriteStringWithModeAsync(this XmlWriter writer, string value, StringSerializationMode mode)
        {
            return mode == StringSerializationMode.HtmlEncoded
                ? writer.WriteStringAsync(value)
                : writer.WriteCDataEscapeAsync(value);
        }

        public static Task WriteStartElementAsync(this XmlWriter writer, XName name)
        {
            return writer.WriteStartElementAsync(null, name.LocalName, name.NamespaceName);
        }

        /// <summary>
        /// Serializes beginning of SOAP envelope into X-Road message.
        /// </summary>
        public static async Task WriteSoapEnvelopeAsync(this XmlWriter writer, IMessageFormatter messageFormatter, ProtocolDefinition protocolDefinition)
        {
            var soapEnvPrefix = protocolDefinition != null ? protocolDefinition.GlobalNamespacePrefixes[messageFormatter.Namespace] : PrefixConstants.SoapEnv;

            await messageFormatter.WriteStartEnvelopeAsync(writer, soapEnvPrefix).ConfigureAwait(false);

            if (protocolDefinition == null)
                return;

            foreach (var kvp in protocolDefinition.GlobalNamespacePrefixes)
                await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, kvp.Value, NamespaceConstants.Xmlns, kvp.Key.NamespaceName).ConfigureAwait(false);

            if (protocolDefinition.Style is RpcEncodedStyle)
                await writer.WriteAttributeStringAsync(null, "encodingStyle", messageFormatter.Namespace, NamespaceConstants.SoapEnc).ConfigureAwait(false);
        }

        public static async Task WriteMissingAttributesAsync(this XmlWriter writer, ProtocolDefinition protocolDefinition)
        {
            if (protocolDefinition == null)
                return;

            foreach (var kvp in protocolDefinition.GlobalNamespacePrefixes)
                if (writer.LookupPrefix(kvp.Key.NamespaceName) == null)
                    await writer.WriteAttributeStringAsync(PrefixConstants.Xmlns, kvp.Value, NamespaceConstants.Xmlns, kvp.Key.NamespaceName).ConfigureAwait(false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace XRoadLib.Extensions
{
    public static class XmlReaderExtensions
    {
        private static readonly XName qnXsiNil = XName.Get("nil", NamespaceConstants.XSI);
        private static readonly XName qnXsiType = XName.Get("type", NamespaceConstants.XSI);
        private static readonly XName qnSoapEncArray = XName.Get("Array", NamespaceConstants.SOAP_ENC);
        private static readonly XName qnSoapEncArrayType = XName.Get("arrayType", NamespaceConstants.SOAP_ENC);

        public static bool ReadToElement(this XmlReader reader, string localName)
        {
            while (reader.LocalName != localName && reader.Read()) { }
            return reader.LocalName == localName;
        }

        public static bool IsNilElement(this XmlReader reader)
        {
            var value = reader.GetAttribute(qnXsiNil.LocalName, qnXsiNil.NamespaceName);

            switch (value)
            {
                case "1":
                case "true":
                    return true;

                case "0":
                case "false":
                case null:
                    return false;

                default:
                    throw new XmlException($"Invalid {qnXsiNil} attribute value: `{value}`");
            }
        }

        internal static Tuple<XName, bool> GetTypeAttributeValue(this XmlReader reader)
        {
            return GetTypeAttributeValue(reader, qnXsiType);
        }

        private static Tuple<XName, bool> GetTypeAttributeValue(XmlReader reader, XName attributeName, bool isArrayType = false)
        {
            var typeValue = reader.GetAttribute(attributeName.LocalName, attributeName.NamespaceName);
            if (typeValue == null)
                return null;

            var namespaceSeparatorIndex = typeValue.IndexOf(':');
            var namespacePrefix = namespaceSeparatorIndex < 0 ? string.Empty : typeValue.Substring(0, namespaceSeparatorIndex);
            var typeName = namespaceSeparatorIndex < 0 ? typeValue : typeValue.Substring(namespaceSeparatorIndex + 1);

            var typeNamespace = reader.LookupNamespace(namespacePrefix);
            if (typeNamespace == null)
                throw XRoadException.InvalidQuery("Undefined namespace prefix `{0}` given in XML message for element `{1}` xsi:type.", namespacePrefix, reader.LocalName);

            if (isArrayType)
                typeName = typeName.Substring(0, typeName.LastIndexOf('['));

            var qualifiedName = XName.Get(typeName, typeNamespace);

            return qualifiedName != qnSoapEncArray ? Tuple.Create(qualifiedName, isArrayType) : GetTypeAttributeValue(reader, qnSoapEncArrayType, true);
        }

        public static void ReadToEndElement(this XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            var currentDepth = reader.Depth;

            while (reader.Read() && currentDepth < reader.Depth)
            { }
        }

        public static bool IsCurrentElement(this XmlReader reader, int depth, string name, string @namespace = "")
        {
            return reader.NodeType == XmlNodeType.Element && reader.Depth == depth && reader.LocalName == name && reader.NamespaceURI == @namespace;
        }

        public static bool MoveToElement(this XmlReader reader, int depth, string name = null, string @namespace = "")
        {
            while (true)
            {
                if (reader.Depth == depth && reader.NodeType == XmlNodeType.Element && (name == null || reader.IsCurrentElement(depth, name, @namespace)))
                    return true;

                if (!reader.Read() || reader.Depth < depth)
                    return false;
            }
        }

        private static readonly ICollection<string> headerNamespaces = new[]
        {
            NamespaceConstants.XTEE,
            NamespaceConstants.XROAD,
            NamespaceConstants.XROAD_V4,
            NamespaceConstants.XROAD_V4_REPR
        };

        public static bool IsHeaderNamespace(this XmlReader reader)
        {
            return headerNamespaces.Contains(reader.NamespaceURI);
        }

        public static void MoveToBody(this XmlReader reader)
        {
            if (!reader.MoveToElement(0, "Envelope", NamespaceConstants.SOAP_ENV))
                throw XRoadException.InvalidQuery("Element `{0}:Envelope` is missing from request content.", NamespaceConstants.SOAP);

            if (!reader.MoveToElement(1, "Body", NamespaceConstants.SOAP_ENV))
                throw XRoadException.InvalidQuery("Element `{0}:Body` is missing from request content.", NamespaceConstants.SOAP);
        }

        public static void MoveToPayload(this XmlReader reader, XName rootElementName)
        {
            reader.MoveToBody();

            if (!reader.MoveToElement(2, rootElementName.LocalName, rootElementName.NamespaceName))
                throw XRoadException.InvalidQuery("Payload is missing from request content.");
        }
    }
}
using System.Collections.Generic;
using System.Xml;

namespace XRoadLib.Extensions
{
    public static class XmlReaderExtensions
    {
        private static readonly XmlQualifiedName qnXsiNil = new XmlQualifiedName("nil", NamespaceConstants.XSI);
        private static readonly XmlQualifiedName qnXsiType = new XmlQualifiedName("type", NamespaceConstants.XSI);
        private static readonly XmlQualifiedName qnSoapEncArray = new XmlQualifiedName("Array", NamespaceConstants.SOAP_ENC);
        private static readonly XmlQualifiedName qnSoapEncArrayType = new XmlQualifiedName("arrayType", NamespaceConstants.SOAP_ENC);

        public static bool ReadToElement(this XmlReader reader, string localName)
        {
            while (reader.LocalName != localName && reader.Read()) { }
            return reader.LocalName == localName;
        }

        public static bool IsNilElement(this XmlReader reader)
        {
            var value = reader.GetAttribute(qnXsiNil.Name, qnXsiNil.Namespace);

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

        public static XmlQualifiedName GetTypeAttributeValue(this XmlReader reader)
        {
            return GetTypeAttributeValue(reader, qnXsiType);
        }

        private static XmlQualifiedName GetTypeAttributeValue(XmlReader reader, XmlQualifiedName attributeName, bool isArrayType = false)
        {
            var typeValue = reader.GetAttribute(attributeName.Name, attributeName.Namespace);
            if (typeValue == null)
                return null;

            var namespaceSeparatorIndex = typeValue.IndexOf(':');
            var namespacePrefix = namespaceSeparatorIndex < 0 ? string.Empty : typeValue.Substring(0, namespaceSeparatorIndex);
            var typeName = namespaceSeparatorIndex < 0 ? typeValue : typeValue.Substring(namespaceSeparatorIndex + 1);

            var typeNamespace = reader.LookupNamespace(namespacePrefix);

            if (isArrayType)
                typeName = typeName.Substring(0, typeName.LastIndexOf('[')) + "[]";

            var qualifiedName = new XmlQualifiedName(typeName, typeNamespace);

            return qualifiedName != qnSoapEncArray ? qualifiedName : GetTypeAttributeValue(reader, qnSoapEncArrayType, true);
        }

        public static void ReadToEndElement(this XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;

            var currentDepth = reader.Depth;
            while (reader.Read() && currentDepth < reader.Depth) {}
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
    }
}
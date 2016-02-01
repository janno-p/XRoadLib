using System.Xml;
using System.Xml.Linq;

namespace XRoadLib.Extensions
{
    public static class XmlWriterExtensions
    {
        private static readonly XmlQualifiedName qnXsiType = new XmlQualifiedName("type", NamespaceConstants.XSI);

        private static void WriteQualifiedAttribute(this XmlWriter writer, string attributeName, string attributeNamespace, string valueName, string valueNamespace)
        {
            writer.WriteStartAttribute(attributeName, attributeNamespace);
            writer.WriteQualifiedName(valueName, valueNamespace);
            writer.WriteEndAttribute();
        }

        public static void WriteTypeAttribute(this XmlWriter writer, string typeName, string typeNamespace)
        {
            writer.WriteQualifiedAttribute(qnXsiType.Name, qnXsiType.Namespace, typeName, typeNamespace);
        }

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

        public static void WriteArrayTypeAttribute(this XmlWriter writer, XName qualifiedName, int arraySize)
        {
            writer.WriteArrayTypeAttribute(qualifiedName.LocalName, qualifiedName.NamespaceName, arraySize);
        }

        public static void WriteNilAttribute(this XmlWriter writer)
        {
            writer.WriteAttributeString("nil", NamespaceConstants.XSI, "1");
        }

        public static void WriteCDataEscape(this XmlWriter writer, string value)
        {
            if (value.Contains("&") || value.Contains("<") || value.Contains(">"))
                writer.WriteCData(value);
            else writer.WriteValue(value);
        }
    }
}
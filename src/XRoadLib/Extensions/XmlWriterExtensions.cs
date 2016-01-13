using System.Xml;

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

        public static void WriteTypeAttribute(this XmlWriter writer, XmlQualifiedName qualifiedName, string ns = null)
        {
            writer.WriteTypeAttribute(qualifiedName.Name, ns ?? qualifiedName.Namespace);
        }

        private static void WriteArrayTypeAttribute(this XmlWriter writer, string typeName, string typeNamespace, int arraySize)
        {
            writer.WriteStartAttribute("arrayType", NamespaceConstants.SOAP_ENC);
            writer.WriteQualifiedName(typeName, typeNamespace);
            writer.WriteString($"[{arraySize}]");
            writer.WriteEndAttribute();
        }

        public static void WriteArrayTypeAttribute(this XmlWriter writer, XmlQualifiedName qualifiedName, int arraySize)
        {
            writer.WriteArrayTypeAttribute(qualifiedName.Name, qualifiedName.Namespace, arraySize);
        }

        public static void WriteXteeHeaderElement(this XmlWriter writer, string name, string value, bool writeRaw = false)
        {
            writer.WriteHeaderElement(XRoadProtocol.Version20.GetNamespace(), name, value, writeRaw);
        }

        public static void WriteHeaderElement(this XmlWriter writer, string @namespace, string name, string value, bool writeRaw = false)
        {
            writer.WriteStartElement(name, @namespace);
            writer.WriteTypeAttribute("string", NamespaceConstants.XSD);

            if (!string.IsNullOrEmpty(value))
            {
                if (writeRaw)
                    writer.WriteRaw(value);
                else
                    writer.WriteValue(value);
            }

            writer.WriteEndElement();
        }

        public static void WriteNilAttribute(this XmlWriter writer)
        {
            writer.WriteAttributeString("nil", NamespaceConstants.XSI, "1");
        }
    }
}
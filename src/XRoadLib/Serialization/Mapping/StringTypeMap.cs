using System;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class StringTypeMap : TypeMap<string>
    {
        private readonly XmlQualifiedName xmlQualifiedName;

        public StringTypeMap(XmlQualifiedName xmlQualifiedName = null)
        {
            this.xmlQualifiedName = xmlQualifiedName ?? new XmlQualifiedName("string", NamespaceConstants.XSD);
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (context.Protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute(xmlQualifiedName);

            var valueString = value.ToString();

            if (valueString.Contains("&") || valueString.Contains("<") || valueString.Contains(">"))
                writer.WriteCData(valueString);
            else writer.WriteValue(valueString);
        }
    }
}
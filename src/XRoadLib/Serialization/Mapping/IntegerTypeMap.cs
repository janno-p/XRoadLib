using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class IntegerTypeMap : TypeMap<int>
    {
        private readonly XName qualifiedName = XName.Get("int", NamespaceConstants.XSD);

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToInt32(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            context.Protocol.Style.WriteExplicitType(writer, qualifiedName);

            writer.WriteValue(value);
        }
    }
}
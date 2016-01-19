using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class FloatTypeMap : TypeMap<float>
    {
        private readonly XName qualifiedName = XName.Get("float", NamespaceConstants.XSD);

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToDouble(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (context.Protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute(qualifiedName);

            writer.WriteValue(value);
        }
    }
}
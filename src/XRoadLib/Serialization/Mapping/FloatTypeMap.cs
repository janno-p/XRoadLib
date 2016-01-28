using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class FloatTypeMap : TypeMap<float>
    {
        public static ITypeMap Instance { get; } = new FloatTypeMap();

        public override XName QualifiedName { get; } = XName.Get("float", NamespaceConstants.XSD);

        private FloatTypeMap()
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToDouble(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            context.Protocol.Style.WriteExplicitType(writer, QualifiedName);

            writer.WriteValue(value);
        }
    }
}
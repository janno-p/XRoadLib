using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class LongTypeMap : TypeMap<long>
    {
        public static ITypeMap Instance { get; } = new LongTypeMap();

        public override XName QualifiedName { get; } = XName.Get("long", NamespaceConstants.XSD);

        private LongTypeMap()
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToInt64(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            context.Protocol.Style.WriteExplicitType(writer, QualifiedName);

            writer.WriteValue(value);
        }
    }
}
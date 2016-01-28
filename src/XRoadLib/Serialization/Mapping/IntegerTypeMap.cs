using System;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class IntegerTypeMap : TypeMap<int>
    {
        public static ITypeMap Instance { get; } = new IntegerTypeMap();

        private readonly XName qualifiedName;

        public IntegerTypeMap(XName qualifiedName = null)
        {
            this.qualifiedName = qualifiedName ?? XName.Get("int", NamespaceConstants.XSD);
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToInt32(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            context.Protocol.Style.WriteExplicitType(writer, qualifiedName);

            writer.WriteValue(value);
        }
    }
}
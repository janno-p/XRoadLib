using System;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class Int32TypeMap : TypeMap<int>
    {
        public Int32TypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToInt32(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, SerializationContext context)
        {
            context.Protocol.Style.WriteExplicitType(writer, TypeDefinition.Name);

            writer.WriteValue(value);
        }
    }
}
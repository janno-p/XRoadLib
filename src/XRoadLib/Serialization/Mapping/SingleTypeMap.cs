using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class SingleTypeMap : TypeMap
    {
        public SingleTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, 0.0f);

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? 0.0f : XmlConvert.ToDouble(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(value);
        }
    }
}
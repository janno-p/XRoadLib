using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class Int32TypeMap : TypeMap
    {
        public Int32TypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, 0);

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? 0 : XmlConvert.ToInt32(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(value);
        }
    }
}
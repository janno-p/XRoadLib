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

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? defaultValue : XmlConvert.ToInt32(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(value);
        }
    }
}
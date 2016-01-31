using System;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class StringTypeMap : TypeMap<string>
    {
        public StringTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            return string.IsNullOrEmpty(value) ? null : value;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            var valueString = value.ToString();

            if (valueString.Contains("&") || valueString.Contains("<") || valueString.Contains(">"))
                writer.WriteCData(valueString);
            else writer.WriteValue(valueString);
        }
    }
}
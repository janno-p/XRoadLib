using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class DecimalTypeMap : TypeMap
    {
        public DecimalTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return reader.MoveNextAndReturn(HandleEmptyElement(content, message));

            var value = reader.ReadElementContentAsString();

            if (string.IsNullOrEmpty(value))
                return HandleEmptyElement(content, message);

            return XmlConvert.ToDecimal(value);
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            message.Style.WriteType(writer, Definition, content);

            writer.WriteValue(value);
        }

        private static decimal? HandleEmptyElement(ContentDefinition content, XRoadMessage message)
        {
            return message.HandleEmptyElementOfValueType<decimal>(content, () => throw new InvalidQueryException("'' is not a valid value for 'decimal'"));
        }
    }
}
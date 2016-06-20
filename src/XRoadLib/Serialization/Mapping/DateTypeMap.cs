using System;
using System.Globalization;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class DateTypeMap : TypeMap
    {
        private static readonly string[] dateFormats =
        {
            "yyyy-MM-dd",
            "'+'yyyy-MM-dd",
            "'-'yyyy-MM-dd",
            "yyyy-MM-ddzzz",
            "'+'yyyy-MM-ddzzz",
            "'-'yyyy-MM-ddzzz",
            "yyyy-MM-dd'Z'",
            "'+'yyyy-MM-dd'Z'",
            "'-'yyyy-MM-dd'Z'"
        };

        public DateTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, new DateTime());

            var value = reader.ReadElementContentAsString();
            if (string.IsNullOrEmpty(value))
                return null;

            var date = DateTime.ParseExact(value, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            if (value[value.Length - 1] == 'Z')
                date = date.ToLocalTime();

            return date;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(XmlConvert.ToString((DateTime)value, "yyyy-MM-dd"));
        }
    }
}
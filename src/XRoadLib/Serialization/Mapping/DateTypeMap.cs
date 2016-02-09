using System;
using System.Data.SqlTypes;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class DateTypeMap : TypeMap<DateTime>
    {
        public DateTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message, bool validateRequired)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            if (string.IsNullOrEmpty(value))
                return null;

            var date = XmlConvert.ToDateTimeOffset(value).Date;

            var minDateTimeValue = (DateTime)SqlDateTime.MinValue;
            if (date == minDateTimeValue || date == DateTime.MinValue)
                return null;

            if (date < minDateTimeValue)
                throw XRoadException.PäringSisaldabVarasematKuupäeva(minDateTimeValue);

            return date;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(XmlConvert.ToString((DateTime)value, "yyyy-MM-dd"));
        }
    }
}
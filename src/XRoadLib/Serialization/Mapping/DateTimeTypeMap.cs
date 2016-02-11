using System;
using System.Data.SqlTypes;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class DateTimeTypeMap : TypeMap
    {
        public DateTimeTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        { }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, IContentDefinition definition, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return MoveNextAndReturn(reader, new DateTime());

            var value = reader.ReadString();

            if (string.IsNullOrEmpty(value) || value.StartsWith("0001-01-01T00:00:00", StringComparison.InvariantCulture))
                return null;

            var dateTime = XmlConvert.ToDateTimeOffset(value).DateTime;

            dateTime = dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));

            var minDateTimeValue = (DateTime)SqlDateTime.MinValue;
            if (dateTime == minDateTimeValue || dateTime == DateTime.MinValue)
                return null;

            if (dateTime < minDateTimeValue)
                throw XRoadException.PäringSisaldabVarasematKuupäeva(minDateTimeValue);

            return dateTime;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, IContentDefinition definition, XRoadMessage message)
        {
            if (!(definition is RequestValueDefinition))
                message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            writer.WriteValue(value);
        }
    }
}
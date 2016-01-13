using System;
using System.Data.SqlTypes;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class DateTimeTypeMap : TypeMap<DateTime>
    {
        private readonly XmlQualifiedName xmlQualifiedName = new XmlQualifiedName("dateTime", NamespaceConstants.XSD);

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
        {
            if (reader.IsEmptyElement)
                return null;

            var value = reader.ReadString();

            if (string.IsNullOrEmpty(value) || value.StartsWith("0001-01-01T00:00:00"))
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

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (context.Protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute(xmlQualifiedName);

            writer.WriteValue(value);
        }
    }
}
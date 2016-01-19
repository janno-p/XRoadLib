using System;
using System.Data.SqlTypes;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class DateTypeMap : TypeMap<DateTime>
    {
        private readonly XName qualifiedName = XName.Get("date", NamespaceConstants.XSD);

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, SerializationContext context)
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

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type fieldType, SerializationContext context)
        {
            if (context.Protocol == XRoadProtocol.Version20)
                writer.WriteTypeAttribute(qualifiedName);

            writer.WriteValue(XmlConvert.ToString((DateTime)value, "yyyy-MM-dd"));
        }
    }
}
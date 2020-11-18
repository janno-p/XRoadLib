using System;
using System.Threading.Tasks;
using System.Xml;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class TimeSpanTypeMap : TypeMap
    {
        public TimeSpanTypeMap(TypeDefinition typeDefinition) : base(typeDefinition) { }

        public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return await reader.MoveNextAndReturnAsync(HandleEmptyElement(content, message)).ConfigureAwait(false);

            var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

            return string.IsNullOrEmpty(value)
                ? HandleEmptyElement(content, message)
                : XmlConvert.ToTimeSpan(value);
        }

        public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

            await writer.WriteStringAsync(XmlConvert.ToString((TimeSpan)value)).ConfigureAwait(false);
        }

        private static TimeSpan? HandleEmptyElement(ContentDefinition content, XRoadMessage message)
        {
            return message.HandleEmptyElementOfValueType<TimeSpan>(content, () => throw new InvalidQueryException("'' is not a valid value for 'duration'"));
        }
    }
}
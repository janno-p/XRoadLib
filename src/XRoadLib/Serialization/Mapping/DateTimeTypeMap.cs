using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

public class DateTimeTypeMap : TypeMap
{
    public DateTimeTypeMap(TypeDefinition typeDefinition) : base(typeDefinition) { }

    public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
    {
        if (reader.IsEmptyElement)
            return await reader.MoveNextAndReturnAsync(HandleEmptyElement(content, message)).ConfigureAwait(false);

        var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(value))
            return HandleEmptyElement(content, message);

        var dateTime = XmlConvert.ToDateTimeOffset(value).DateTime;

        return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
    }

    public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
    {
        await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

        await writer.WriteStringAsync(XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.Unspecified)).ConfigureAwait(false);
    }

    private static DateTime? HandleEmptyElement(ContentDefinition content, XRoadMessage message)
    {
        return message.HandleEmptyElementOfValueType<DateTime>(content, () => throw new InvalidQueryException("'' is not a valid value for 'dateTime'"));
    }
}
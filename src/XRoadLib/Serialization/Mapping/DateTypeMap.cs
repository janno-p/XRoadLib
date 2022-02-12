using System.Globalization;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

public class DateTypeMap : TypeMap
{
    private static readonly string[] DateFormats =
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

    public DateTypeMap(TypeDefinition typeDefinition) : base(typeDefinition) { }

    public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
    {
        if (reader.IsEmptyElement)
            return await reader.MoveNextAndReturnAsync(HandleEmptyElement(content, message)).ConfigureAwait(false);

        var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(value))
            return HandleEmptyElement(content, message);

        var date = DateTime.ParseExact(value, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None);
        if (value[value.Length - 1] == 'Z')
            date = date.ToLocalTime();

        return date;
    }

    public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
    {
        await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

        await writer.WriteStringAsync(XmlConvert.ToString((DateTime)value, "yyyy-MM-dd")).ConfigureAwait(false);
    }

    private static DateTime? HandleEmptyElement(ContentDefinition content, XRoadMessage message)
    {
        return message.HandleEmptyElementOfValueType<DateTime>(content, () => throw new InvalidQueryException("'' is not a valid value for 'date'"));
    }
}
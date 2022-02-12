using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

/// <summary>
/// Serialization/deserialization details of `string` type.
/// </summary>
public class StringTypeMap : TypeMap
{
    /// <summary>
    /// Initializes new string type map.
    /// </summary>
    public StringTypeMap(TypeDefinition typeDefinition)
        : base(typeDefinition)
    { }

    /// <summary>
    /// String deserialization logic.
    /// </summary>
    public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
    {
        if (reader.IsEmptyElement)
            return await reader.MoveNextAndReturnAsync("").ConfigureAwait(false);

        var value = await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);

        return string.IsNullOrEmpty(value) ? "" : value;
    }

    /// <summary>
    /// String serialization logic.
    /// </summary>
    public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
    {
        await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

        await writer.WriteStringWithModeAsync(value.ToString(), message.Style.StringSerializationMode).ConfigureAwait(false);
    }
}
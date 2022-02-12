namespace XRoadLib.Wsdl;

public class MimeContentBinding : ServiceDescriptionFormatExtension
{
    public string Part { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(PrefixConstants.Mime, "content", NamespaceConstants.Mime).ConfigureAwait(false);
        await writer.WriteAttributeStringAsync(null, "part", null, Part).ConfigureAwait(false);
        await writer.WriteAttributeStringAsync(null, "type", null, Type).ConfigureAwait(false);
        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
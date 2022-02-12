namespace XRoadLib.Wsdl;

public class MimeMultipartRelatedBinding : ServiceDescriptionFormatExtension
{
    [UsedImplicitly]
    public List<MimePart> Parts { get; } = new();

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(PrefixConstants.Mime, "multipartRelated", NamespaceConstants.Mime).ConfigureAwait(false);

        foreach (var part in Parts)
            await part.WriteAsync(writer).ConfigureAwait(false);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
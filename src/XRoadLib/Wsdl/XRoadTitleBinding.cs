namespace XRoadLib.Wsdl;

[UsedImplicitly]
public class XRoadTitleBinding : ServiceDescriptionFormatExtension
{
    [UsedImplicitly]
    public string Prefix { get; }

    [UsedImplicitly]
    public string Namespace { get; }

    [UsedImplicitly]
    public string Text { get; set; } = string.Empty;

    [UsedImplicitly]
    public string? Language { get; set; }

    public XRoadTitleBinding(string prefix, string ns)
    {
        Prefix = prefix;
        Namespace = ns;
    }

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(Prefix, "title", Namespace).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(Language))
            await writer.WriteAttributeStringAsync(null, "lang", NamespaceConstants.Xml, Language).ConfigureAwait(false);

        await writer.WriteStringAsync(Text).ConfigureAwait(false);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
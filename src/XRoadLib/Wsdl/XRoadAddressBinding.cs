namespace XRoadLib.Wsdl;

[UsedImplicitly]
public class XRoadAddressBinding : ServiceDescriptionFormatExtension
{
    [UsedImplicitly]
    public string Prefix { get; }

    [UsedImplicitly]
    public string Namespace { get; }

    [UsedImplicitly]
    public string Producer { get; set; } = string.Empty;

    public XRoadAddressBinding(string prefix, string ns)
    {
        Prefix = prefix;
        Namespace = ns;
    }

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(Prefix, "address", Namespace).ConfigureAwait(false);
        await writer.WriteAttributeStringAsync(null, "producer", null, Producer).ConfigureAwait(false);
        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
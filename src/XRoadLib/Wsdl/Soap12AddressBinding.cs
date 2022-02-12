namespace XRoadLib.Wsdl;

[UsedImplicitly]
public class Soap12AddressBinding : ServiceDescriptionFormatExtension
{
    [UsedImplicitly]
    public string Location { get; set; } = string.Empty;

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(PrefixConstants.Soap12, "address", NamespaceConstants.Soap12).ConfigureAwait(false);
        await writer.WriteAttributeStringAsync(null, "location", null, Location).ConfigureAwait(false);
        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
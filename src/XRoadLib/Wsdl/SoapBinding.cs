namespace XRoadLib.Wsdl;

public class SoapBinding : ServiceDescriptionFormatExtension
{
    public SoapBindingStyle Style { get; set; } = SoapBindingStyle.Default;
    public string Transport { get; set; } = string.Empty;

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(PrefixConstants.Soap, "binding", NamespaceConstants.Soap).ConfigureAwait(false);

        if (Style != SoapBindingStyle.Default)
            await writer.WriteAttributeStringAsync(null, "style", null, Style == SoapBindingStyle.Rpc ? "rpc" : "document").ConfigureAwait(false);

        await writer.WriteAttributeStringAsync(null, "transport", null, Transport).ConfigureAwait(false);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
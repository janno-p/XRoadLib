using XRoadLib.Extensions;

namespace XRoadLib.Wsdl;

public class SoapHeaderBinding : ServiceDescriptionFormatExtension
{
    public string Encoding { get; set; }
    public XmlQualifiedName Message { get; set; }
    public string Namespace { get; set; }
    public string Part { get; set; }
    public SoapBindingUse Use { get; set; } = SoapBindingUse.Default;

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(PrefixConstants.Soap, "header", NamespaceConstants.Soap).ConfigureAwait(false);

        await writer.WriteQualifiedAttributeAsync("message", Message).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(Encoding))
            await writer.WriteAttributeStringAsync(null, "encodingStyle", null, Encoding).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(Namespace))
            await writer.WriteAttributeStringAsync(null, "namespace", null, Namespace).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(Part))
            await writer.WriteAttributeStringAsync(null, "part", null, Part).ConfigureAwait(false);

        if (Use != SoapBindingUse.Default)
            await writer.WriteAttributeStringAsync(null, "use", null, Use == SoapBindingUse.Encoded ? "encoded" : "literal").ConfigureAwait(false);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
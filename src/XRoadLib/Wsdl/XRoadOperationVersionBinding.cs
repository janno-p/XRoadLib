namespace XRoadLib.Wsdl;

public class XRoadOperationVersionBinding : ServiceDescriptionFormatExtension
{
    [UsedImplicitly]
    public string Prefix { get; }

    [UsedImplicitly]
    public string Namespace { get; }

    public string Version { get; set; }

    public XRoadOperationVersionBinding(string prefix, string ns, string version)
    {
        Prefix = prefix;
        Namespace = ns;
        Version = version;
    }

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(Prefix, "version", Namespace).ConfigureAwait(false);
        await writer.WriteStringAsync(Version).ConfigureAwait(false);
        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
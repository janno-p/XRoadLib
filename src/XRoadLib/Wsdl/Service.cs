namespace XRoadLib.Wsdl;

public class Service : NamedItem
{
    protected override string ElementName => "service";

    [UsedImplicitly]
    public List<Port> Ports { get; } = new();

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        foreach (var port in Ports)
            await port.WriteAsync(writer).ConfigureAwait(false);
    }
}
namespace XRoadLib.Wsdl;

public class Message : NamedItem
{
    protected override string ElementName => "message";

    public List<MessagePart> Parts { get; } = new();

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        foreach (var part in Parts)
            await part.WriteAsync(writer).ConfigureAwait(false);
    }
}
namespace XRoadLib.Wsdl;

public class Operation : NamedItem
{
    protected override string ElementName => "operation";

    [UsedImplicitly]
    public List<OperationMessage> Messages { get; } = new();

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        foreach (var message in Messages)
            await message.WriteAsync(writer).ConfigureAwait(false);
    }
}
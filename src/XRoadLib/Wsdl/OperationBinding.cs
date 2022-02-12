namespace XRoadLib.Wsdl;

public class OperationBinding : NamedItem
{
    protected override string ElementName => "operation";

    public InputBinding Input { get; set; }
    public OutputBinding Output { get; set; }

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        if (Input != null)
            await Input.WriteAsync(writer).ConfigureAwait(false);

        if (Output != null)
            await Output.WriteAsync(writer).ConfigureAwait(false);
    }
}
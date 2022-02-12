namespace XRoadLib.Wsdl;

public abstract class NamedItem : DocumentableItem
{
    public string Name { get; set; }

    protected override async Task WriteAttributesAsync(XmlWriter writer)
    {
        await base.WriteAttributesAsync(writer).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(Name))
            await writer.WriteAttributeStringAsync(null, "name", null, Name).ConfigureAwait(false);
    }
}
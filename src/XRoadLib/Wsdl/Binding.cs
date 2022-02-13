using XRoadLib.Extensions;

namespace XRoadLib.Wsdl;

public class Binding : NamedItem
{
    protected override string ElementName => "binding";

    public List<OperationBinding> Operations { get; } = new();
    public XmlQualifiedName? Type { get; set; }

    protected override async Task WriteAttributesAsync(XmlWriter writer)
    {
        await base.WriteAttributesAsync(writer).ConfigureAwait(false);
        await writer.WriteQualifiedAttributeAsync("type", Type).ConfigureAwait(false);
    }

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        foreach (var operation in Operations)
            await operation.WriteAsync(writer).ConfigureAwait(false);
    }
}
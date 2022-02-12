using XRoadLib.Extensions;

namespace XRoadLib.Wsdl;

public abstract class OperationMessage : NamedItem
{
    public XmlQualifiedName Message { get; set; }

    protected override async Task WriteAttributesAsync(XmlWriter writer)
    {
        await base.WriteAttributesAsync(writer).ConfigureAwait(false);
        await writer.WriteQualifiedAttributeAsync("message", Message).ConfigureAwait(false);
    }
}
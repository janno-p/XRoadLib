namespace XRoadLib.Wsdl;

public class ServiceDescription : NamedItem
{
    protected override string ElementName => "definitions";

    public List<Binding> Bindings { get; } = new();
    public List<Message> Messages { get; } = new();
    public List<PortType> PortTypes { get; } = new();
    public List<Service> Services { get; } = new();
    public string? TargetNamespace { get; set; }
    public Types Types { get; } = new();

    protected override async Task WriteAttributesAsync(XmlWriter writer)
    {
        await base.WriteAttributesAsync(writer).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(TargetNamespace))
            await writer.WriteAttributeStringAsync(null, "targetNamespace", null, TargetNamespace).ConfigureAwait(false);
    }

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        // wsdl:import

        if (Types != null)
            await Types.WriteAsync(writer).ConfigureAwait(false);

        foreach (var message in Messages)
            await message.WriteAsync(writer).ConfigureAwait(false);

        foreach (var portType in PortTypes)
            await portType.WriteAsync(writer).ConfigureAwait(false);

        foreach (var binding in Bindings)
            await binding.WriteAsync(writer).ConfigureAwait(false);

        foreach (var service in Services)
            await service.WriteAsync(writer).ConfigureAwait(false);
    }
}
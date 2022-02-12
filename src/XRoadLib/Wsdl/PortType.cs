﻿namespace XRoadLib.Wsdl;

public class PortType : NamedItem
{
    protected override string ElementName => "portType";

    public List<Operation> Operations { get; } = new();

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        foreach (var operation in Operations)
            await operation.WriteAsync(writer).ConfigureAwait(false);
    }
}
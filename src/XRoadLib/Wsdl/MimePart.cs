﻿namespace XRoadLib.Wsdl;

public class MimePart : ServiceDescriptionFormatExtension
{
    public List<ServiceDescriptionFormatExtension> Extensions { get; } = new();

    internal override async Task WriteAsync(XmlWriter writer)
    {
        await writer.WriteStartElementAsync(PrefixConstants.Mime, "part", NamespaceConstants.Mime).ConfigureAwait(false);

        foreach (var extension in Extensions)
            await extension.WriteAsync(writer).ConfigureAwait(false);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
﻿using XRoadLib.Serialization;
using XRoadLib.Wsdl;

namespace XRoadLib.Extensions;

public static class ServiceDescriptionExtensions
{
    /// <summary>
    /// Outputs service description to specified stream.
    /// </summary>
    public static async Task WriteAsync(this ServiceDescription serviceDescription, Stream stream)
    {
        var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Async = true,
            Encoding = XRoadEncoding.Utf8,
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n"
        });

        await writer.WriteStartDocumentAsync().ConfigureAwait(false);
        await serviceDescription.WriteAsync(writer).ConfigureAwait(false);
        await writer.WriteEndDocumentAsync().ConfigureAwait(false);

        await writer.FlushAsync().ConfigureAwait(false);
    }
}
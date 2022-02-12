using System.Xml.Schema;

namespace XRoadLib.Wsdl;

public class Types : DocumentableItem
{
    protected override string ElementName => "types";

    public List<XmlSchema> Schemas { get; } = new();

    protected override async Task WriteElementsAsync(XmlWriter writer)
    {
        await base.WriteElementsAsync(writer).ConfigureAwait(false);

        foreach (var schema in Schemas)
        {
            using var stream = new MemoryStream();
            schema.Write(stream);
            stream.Seek(0, SeekOrigin.Begin);

            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });

            while (await reader.ReadAsync().ConfigureAwait(false) && reader.NodeType != XmlNodeType.Element)
            {
                // Do nothing
            }

            await writer.WriteNodeAsync(reader, true).ConfigureAwait(false);
        }
    }
}
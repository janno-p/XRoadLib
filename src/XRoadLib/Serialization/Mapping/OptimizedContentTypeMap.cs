using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping;

public class OptimizedContentTypeMap : ITypeMap
{
    public TypeDefinition Definition { get; }

    [UsedImplicitly]
    public OptimizedContentTypeMap(ContentTypeMap contentTypeMap)
    {
        Definition = contentTypeMap.Definition;
    }

    public async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
    {
        if (reader.IsEmptyElement)
            return await DeserializeBase64ContentAsync(reader, message).ConfigureAwait(false);

        if (!await reader.ReadToContentAsync().ConfigureAwait(false))
        {
            if (reader.NodeType == XmlNodeType.EndElement)
                return GetEmptyAttachmentStream(message);

            throw new InvalidQueryException("Invalid content element.");
        }

        if (reader.NodeType != XmlNodeType.Element)
            return await DeserializeBase64ContentAsync(reader, message).ConfigureAwait(false);

        if (!await reader.MoveToElementAsync(reader.Depth, XName.Get("Include", NamespaceConstants.Xop)).ConfigureAwait(false))
            throw new InvalidQueryException("Missing `xop:Include` reference to multipart content.");

        var contentId = reader.GetAttribute("href");
        if (string.IsNullOrWhiteSpace(contentId))
            throw new InvalidQueryException("Missing `href` attribute to multipart content.");

        var attachment = message.GetAttachment(contentId.Substring(4));
        if (attachment == null)
            throw new InvalidQueryException($"MIME multipart message does not contain message part with ID `{contentId}`.");

        return attachment.ContentStream;
    }

    private static Stream GetEmptyAttachmentStream(IAttachmentManager attachmentManager)
    {
        var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
        attachmentManager.AllAttachments.Add(tempAttachment);
        return tempAttachment.ContentStream;
    }

    private static async Task<object> DeserializeBase64ContentAsync(XmlReader reader, IAttachmentManager attachmentManager)
    {
        if (reader.IsEmptyElement)
            return await reader.MoveNextAndReturnAsync(GetEmptyAttachmentStream(attachmentManager)).ConfigureAwait(false);

        const int bufferSize = 1000;

        int bytesRead;
        var buffer = new byte[bufferSize];

        var contentStream = GetEmptyAttachmentStream(attachmentManager);

        while ((bytesRead = await reader.ReadContentAsBase64Async(buffer, 0, bufferSize).ConfigureAwait(false)) > 0)
            await contentStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

        return contentStream;
    }

    public async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
    {
        var attachment = new XRoadAttachment((Stream)value);
        message.AllAttachments.Add(attachment);

        await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

        await writer.WriteStartElementAsync(PrefixConstants.Xop, "Include", NamespaceConstants.Xop).ConfigureAwait(false);

        await writer.WriteAttributeStringAsync(null, "href", null, $"cid:{attachment.ContentId}").ConfigureAwait(false);

        await writer.WriteEndElementAsync().ConfigureAwait(false);
    }
}
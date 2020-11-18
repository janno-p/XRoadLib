using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ContentTypeMap : TypeMap, IContentTypeMap
    {
        private readonly XName _encodedTypeName;
        private readonly ITypeMap _optimizedContentTypeMap;

        public ContentTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            _encodedTypeName = XName.Get(Definition.Name.LocalName, NamespaceConstants.SoapEnc);
            _optimizedContentTypeMap = new OptimizedContentTypeMap(this);
        }

        public ITypeMap GetOptimizedContentTypeMap()
        {
            return _optimizedContentTypeMap;
        }

        public override async Task<object> DeserializeAsync(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var contentId = reader.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(contentId))
            {
                if (message.IsMultipartContainer)
                    throw new InvalidQueryException("Missing `href` attribute to multipart content.");

                var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
                message.AllAttachments.Add(tempAttachment);

                if (reader.IsEmptyElement)
                    return await reader.MoveNextAndReturnAsync(tempAttachment.ContentStream).ConfigureAwait(false);

                await reader.ReadAsync().ConfigureAwait(false);

                const int bufferSize = 1000;

                int bytesRead;
                var buffer = new byte[bufferSize];

                while ((bytesRead = await reader.ReadContentAsBase64Async(buffer, 0, bufferSize).ConfigureAwait(false)) > 0)
                    await tempAttachment.ContentStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);

                return tempAttachment.ContentStream;
            }

            var attachment = message.GetAttachment(contentId.Substring(4));
            if (attachment == null)
                throw new InvalidQueryException($"MIME multipart message does not contain message part with ID `{contentId}`.");

            if (reader.IsEmptyElement)
                return await reader.MoveNextAndReturnAsync(attachment.ContentStream).ConfigureAwait(false);

            await reader.ReadToEndElementAsync().ConfigureAwait(false);

            return attachment.ContentStream;
        }

        public override async Task SerializeAsync(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            var attachment = new XRoadAttachment((Stream)value);
            message.AllAttachments.Add(attachment);

            if (message.BinaryMode == BinaryMode.Attachment)
            {
                if (!(content.Particle is RequestDefinition))
                    await message.Style.WriteExplicitTypeAsync(writer, _encodedTypeName).ConfigureAwait(false);

                await writer.WriteAttributeStringAsync(null, "href", null, $"cid:{attachment.ContentId}").ConfigureAwait(false);
                return;
            }

            await message.Style.WriteTypeAsync(writer, Definition, content).ConfigureAwait(false);

            attachment.IsMultipartContent = false;

            await attachment.WriteAsBase64Async(writer).ConfigureAwait(false);
        }
    }
}

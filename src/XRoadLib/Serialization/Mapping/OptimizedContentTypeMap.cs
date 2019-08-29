using System.IO;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class OptimizedContentTypeMap : ITypeMap
    {
        public TypeDefinition Definition { get; }

        public OptimizedContentTypeMap(ContentTypeMap contentTypeMap)
        {
            Definition = contentTypeMap.Definition;
        }

        public object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return DeserializeBase64Content(reader, message);

            if (!reader.ReadToContent())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                    return GetEmptyAttachmentStream(message);

                throw new InvalidQueryException("Invalid content element.");
            }

            if (reader.NodeType != XmlNodeType.Element)
                return DeserializeBase64Content(reader, message);

            if (!reader.MoveToElement(reader.Depth, XName.Get("Include", NamespaceConstants.XOP)))
                throw new InvalidQueryException("Missing `xop:Include` reference to multipart content.");

            var contentID = reader.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(contentID))
                throw new InvalidQueryException("Missing `href` attribute to multipart content.");

            var attachment = message.GetAttachment(contentID.Substring(4));
            if (attachment == null)
                throw new InvalidQueryException($"MIME multipart message does not contain message part with ID `{contentID}`.");

            return attachment.ContentStream;
        }

        private static Stream GetEmptyAttachmentStream(IAttachmentManager attachmentManager)
        {
            var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
            attachmentManager.AllAttachments.Add(tempAttachment);
            return tempAttachment.ContentStream;
        }

        private static object DeserializeBase64Content(XmlReader reader, XRoadMessage message)
        {
            if (reader.IsEmptyElement)
                return reader.MoveNextAndReturn(GetEmptyAttachmentStream(message));

            const int bufferSize = 1000;

            int bytesRead;
            var buffer = new byte[bufferSize];

            var contentStream = GetEmptyAttachmentStream(message);

            while ((bytesRead = reader.ReadContentAsBase64(buffer, 0, bufferSize)) > 0)
                contentStream.Write(buffer, 0, bytesRead);

            return contentStream;
        }

        public void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            var attachment = new XRoadAttachment((Stream)value);
            message.AllAttachments.Add(attachment);

            message.Style.WriteType(writer, Definition, content);

            writer.WriteStartElement(PrefixConstants.XOP, "Include", NamespaceConstants.XOP);

            writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");

            writer.WriteEndElement();
        }
    }
}
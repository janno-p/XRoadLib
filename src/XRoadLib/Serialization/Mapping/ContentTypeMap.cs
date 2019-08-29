using System.IO;
using System.Xml;
using System.Xml.Linq;
using XRoadLib.Extensions;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ContentTypeMap : TypeMap, IContentTypeMap
    {
        private readonly XName encodedTypeName;
        private readonly ITypeMap optimizedContentTypeMap;

        public ContentTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            encodedTypeName = XName.Get(Definition.Name.LocalName, NamespaceConstants.SOAP_ENC);
            optimizedContentTypeMap = new OptimizedContentTypeMap(this);
        }

        public ITypeMap GetOptimizedContentTypeMap()
        {
            return optimizedContentTypeMap;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, ContentDefinition content, XRoadMessage message)
        {
            var contentID = reader.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(contentID))
            {
                if (message.IsMultipartContainer)
                    throw new InvalidQueryException("Missing `href` attribute to multipart content.");

                var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
                message.AllAttachments.Add(tempAttachment);

                if (reader.IsEmptyElement)
                    return reader.MoveNextAndReturn(tempAttachment.ContentStream);

                reader.Read();

                const int bufferSize = 1000;

                int bytesRead;
                var buffer = new byte[bufferSize];

                while ((bytesRead = reader.ReadContentAsBase64(buffer, 0, bufferSize)) > 0)
                    tempAttachment.ContentStream.Write(buffer, 0, bytesRead);

                return tempAttachment.ContentStream;
            }

            var attachment = message.GetAttachment(contentID.Substring(4));
            if (attachment == null)
                throw new InvalidQueryException($"MIME multipart message does not contain message part with ID `{contentID}`.");

            if (reader.IsEmptyElement)
                return reader.MoveNextAndReturn(attachment.ContentStream);

            reader.ReadToEndElement();

            return attachment.ContentStream;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, ContentDefinition content, XRoadMessage message)
        {
            var attachment = new XRoadAttachment((Stream)value);
            message.AllAttachments.Add(attachment);

            if (message.BinaryMode == BinaryMode.Attachment)
            {
                if (!(content.Particle is RequestDefinition))
                    message.Style.WriteExplicitType(writer, encodedTypeName);

                writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");
                return;
            }

            message.Style.WriteType(writer, Definition, content);

            attachment.IsMultipartContent = false;
            attachment.WriteAsBase64(writer);
        }
    }
}

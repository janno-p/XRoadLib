using System;
using System.IO;
using System.Xml;
using XRoadLib.Schema;
using XRoadLib.Serialization.Template;

namespace XRoadLib.Serialization.Mapping
{
    public class ContentTypeMap : TypeMap<XRoadAttachment>, IContentTypeMap
    {
        private readonly ITypeMap optimizedContentTypeMap;

        public ContentTypeMap(TypeDefinition typeDefinition)
            : base(typeDefinition)
        {
            optimizedContentTypeMap = new OptimizedContentTypeMap(this);
        }

        public ITypeMap GetOptimizedContentTypeMap()
        {
            return optimizedContentTypeMap;
        }

        public override object Deserialize(XmlReader reader, IXmlTemplateNode templateNode, XRoadMessage message)
        {
            var contentID = reader.GetAttribute("href");

            if (string.IsNullOrWhiteSpace(contentID))
            {
                if (message.IsMultipartContainer)
                    throw XRoadException.InvalidQuery("Missing `href` attribute to multipart content.");

                var tempAttachment = new XRoadAttachment(new MemoryStream()) { IsMultipartContent = false };
                message.AllAttachments.Add(tempAttachment);

                if (reader.IsEmptyElement || !reader.Read())
                    return tempAttachment.ContentStream;

                const int bufferSize = 1000;

                int bytesRead;
                var buffer = new byte[bufferSize];

                while ((bytesRead = reader.ReadContentAsBase64(buffer, 0, bufferSize)) > 0)
                    tempAttachment.ContentStream.Write(buffer, 0, bytesRead);

                return tempAttachment.ContentStream;
            }

            var attachment = message.GetAttachment(contentID.Substring(4));
            if (attachment == null)
                throw XRoadException.PäringusPuudubAttachment(contentID);

            return attachment.ContentStream;
        }

        public override void Serialize(XmlWriter writer, IXmlTemplateNode templateNode, object value, Type expectedType, XRoadMessage message)
        {
            var attachment = new XRoadAttachment((Stream)value);
            message.AllAttachments.Add(attachment);

            message.Protocol.Style.WriteExplicitType(writer, Definition.Name);

            if (message.BinaryMode == BinaryMode.Attachment)
            {
                writer.WriteAttributeString("href", $"cid:{attachment.ContentID}");
                return;
            }

            attachment.IsMultipartContent = false;
            attachment.WriteAsBase64(writer);
        }
    }
}
